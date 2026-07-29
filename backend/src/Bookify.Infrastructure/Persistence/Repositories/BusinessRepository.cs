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

    public async Task<Business?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.Providers).ThenInclude(p => p.User)
            .Include(b => b.Services)
            .Include(b => b.Images)
            .Include(b => b.BusinessCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Slug == slug.ToLowerInvariant().Trim(), cancellationToken);
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

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(b =>
                b.Name.Contains(searchTerm) ||
                (b.Description != null && b.Description.Contains(searchTerm)));

        if (categoryId.HasValue)
            query = query.Where(b => b.BusinessCategories.Any(bc => bc.CategoryId == categoryId.Value));

        if (minRating.HasValue)
            query = query.Where(b => b.AverageRating >= minRating.Value);

        if (isVerified.HasValue)
            query = query.Where(b => b.IsVerified == isVerified.Value);

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
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(b =>
                b.Name.Contains(searchTerm) ||
                (b.Description != null && b.Description.Contains(searchTerm)));

        if (categoryId.HasValue)
            query = query.Where(b => b.BusinessCategories.Any(bc => bc.CategoryId == categoryId.Value));

        return await query.CountAsync(cancellationToken);
    }
}
