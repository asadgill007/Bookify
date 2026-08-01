using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

public class BusinessRepository : BaseRepository<Business>, IBusinessRepository
{
    public BusinessRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<int> GetCountAsync(bool? verifiedFilter = null, bool? activeFilter = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (verifiedFilter.HasValue)
            query = query.Where(b => b.IsVerified == verifiedFilter.Value);
        if (activeFilter.HasValue)
            query = query.Where(b => b.IsActive == activeFilter.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Business>> GetFilteredAsync(
        bool? verifiedFilter, bool? activeFilter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (verifiedFilter.HasValue)
            query = query.Where(b => b.IsVerified == verifiedFilter.Value);
        if (activeFilter.HasValue)
            query = query.Where(b => b.IsActive == activeFilter.Value);
        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFilteredCountAsync(bool? verifiedFilter, bool? activeFilter, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (verifiedFilter.HasValue)
            query = query.Where(b => b.IsVerified == verifiedFilter.Value);
        if (activeFilter.HasValue)
            query = query.Where(b => b.IsActive == activeFilter.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Business>> GetByStatusAsync(
        Domain.Enums.VerificationStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(b => b.VerificationStatus == status)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(Domain.Enums.VerificationStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(b => b.VerificationStatus == status, cancellationToken);
    }

    public async Task<Business?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.Providers).ThenInclude(p => p.User)
            .Include(b => b.Services)
            .Include(b => b.Images)
            .Include(b => b.BusinessCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Slug == slug.ToLowerInvariant().Trim(), cancellationToken);
    }

    public async Task<Business?> GetByIdWithDetailsAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.Providers).ThenInclude(p => p.User)
            .Include(b => b.Services)
            .Include(b => b.Images)
            .Include(b => b.BusinessCategories).ThenInclude(bc => bc.Category)
            .Include(b => b.BusinessHours)
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken);
    }

    public async Task AddImageAsync(BusinessImage image, CancellationToken cancellationToken = default)
    {
        await Context.BusinessImages.AddAsync(image, cancellationToken);
    }

    public async Task<IReadOnlyList<Business>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(b => b.Services)
            .Include(b => b.Providers)
            .Include(b => b.Images)
            .Where(b => b.OwnerId == ownerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Business>> SearchAsync(
        string? searchTerm,
        Guid? categoryId,
        double? latitude,
        double? longitude,
        double? radiusKm,
        double? minRating,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isVerified,
        string? sortBy,
        string sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(b => b.BusinessCategories).ThenInclude(bc => bc.Category)
            .Include(b => b.Images.Where(i => i.IsCover))
            .AsQueryable();

        // Case-insensitive across providers (InMemory in dev, SQL Server in prod).
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(term) ||
                (b.Description != null && b.Description.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
            query = query.Where(b => b.BusinessCategories.Any(bc => bc.CategoryId == categoryId.Value));

        if (minRating.HasValue)
            query = query.Where(b => b.AverageRating >= minRating.Value);

        // Price range: match businesses that have at least one active service
        // whose price falls within [minPrice, maxPrice].
        if (minPrice.HasValue)
            query = query.Where(b => b.Services.Any(s => s.IsActive && s.PriceAmount >= minPrice.Value));
        if (maxPrice.HasValue)
            query = query.Where(b => b.Services.Any(s => s.IsActive && s.PriceAmount <= maxPrice.Value));

        if (isVerified.HasValue)
            query = query.Where(b => b.IsVerified == isVerified.Value);

        // Distance radius: approximate with a lat/lon bounding box so the
        // filter is translatable on both SQL Server and the InMemory provider.
        if (latitude.HasValue && longitude.HasValue && radiusKm.HasValue && radiusKm.Value > 0)
        {
            const double kmPerDegree = 111.0;
            var latDelta = radiusKm.Value / kmPerDegree;
            var lonDelta = radiusKm.Value / (kmPerDegree * Math.Cos(latitude.Value * Math.PI / 180.0));
            query = query.Where(b =>
                b.Latitude.HasValue && b.Longitude.HasValue &&
                b.Latitude.Value >= latitude.Value - latDelta && b.Latitude.Value <= latitude.Value + latDelta &&
                b.Longitude.Value >= longitude.Value - lonDelta && b.Longitude.Value <= longitude.Value + lonDelta);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "rating" => sortDirection == "asc"
                ? query.OrderBy(b => b.AverageRating)
                : query.OrderByDescending(b => b.AverageRating),
            "name" => sortDirection == "asc"
                ? query.OrderBy(b => b.Name)
                : query.OrderByDescending(b => b.Name),
            "createdat" => sortDirection == "asc"
                ? query.OrderBy(b => b.CreatedAt)
                : query.OrderByDescending(b => b.CreatedAt),
            _ => query.OrderByDescending(b => b.AverageRating)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SearchCountAsync(
        string? searchTerm,
        Guid? categoryId,
        double? latitude,
        double? longitude,
        double? radiusKm,
        bool? isVerified = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        // Case-insensitive across providers (InMemory in dev, SQL Server in prod).
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(term) ||
                (b.Description != null && b.Description.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
            query = query.Where(b => b.BusinessCategories.Any(bc => bc.CategoryId == categoryId.Value));

        if (minPrice.HasValue)
            query = query.Where(b => b.Services.Any(s => s.IsActive && s.PriceAmount >= minPrice.Value));
        if (maxPrice.HasValue)
            query = query.Where(b => b.Services.Any(s => s.IsActive && s.PriceAmount <= maxPrice.Value));

        if (isVerified.HasValue)
            query = query.Where(b => b.IsVerified == isVerified.Value);

        // Bounding-box radius filter (mirrors SearchAsync).
        if (latitude.HasValue && longitude.HasValue && radiusKm.HasValue && radiusKm.Value > 0)
        {
            const double kmPerDegree = 111.0;
            var latDelta = radiusKm.Value / kmPerDegree;
            var lonDelta = radiusKm.Value / (kmPerDegree * Math.Cos(latitude.Value * Math.PI / 180.0));
            query = query.Where(b =>
                b.Latitude.HasValue && b.Longitude.HasValue &&
                b.Latitude.Value >= latitude.Value - latDelta && b.Latitude.Value <= latitude.Value + latDelta &&
                b.Longitude.Value >= longitude.Value - lonDelta && b.Longitude.Value <= longitude.Value + lonDelta);
        }

        return await query.CountAsync(cancellationToken);
    }
}
