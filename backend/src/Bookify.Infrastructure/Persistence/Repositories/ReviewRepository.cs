using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

public class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<int> GetCountAsync(bool? publishedFilter = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (publishedFilter.HasValue)
            query = query.Where(r => r.IsPublished == publishedFilter.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetFilteredAsync(
        bool? publishedFilter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(r => r.Customer)
            .Include(r => r.Business)
            .AsNoTracking()
            .AsQueryable();

        if (publishedFilter.HasValue)
            query = query.Where(r => r.IsPublished == publishedFilter.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetByBusinessIdAsync(
        Guid businessId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Customer)
            .Include(r => r.Provider)
            .Where(r => r.BusinessId == businessId && r.IsPublished && !r.IsHidden)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetBusinessReviewCountAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(r => r.BusinessId == businessId && r.IsPublished && !r.IsHidden, cancellationToken);

    public async Task<IReadOnlyList<Review>> GetByProviderIdAsync(
        Guid providerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Customer)
            .Include(r => r.Business)
            .Where(r => r.ProviderId == providerId && r.IsPublished && !r.IsHidden)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetProviderReviewCountAsync(Guid providerId, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(r => r.ProviderId == providerId && r.IsPublished && !r.IsHidden, cancellationToken);

    public async Task<bool> HasReviewForAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(r => r.AppointmentId == appointmentId, cancellationToken);

    public async Task<(double AverageRating, int TotalReviews)> GetBusinessRatingAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var stats = await DbSet
            .Where(r => r.BusinessId == businessId && r.IsPublished && !r.IsHidden && !r.IsDeleted)
            .GroupBy(r => 1)
            .Select(g => new
            {
                Average = g.Average(r => (double)r.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats != null
            ? (Math.Round(stats.Average, 1), stats.Count)
            : (0.0, 0);
    }

    public async Task<ReviewStatisticsResult> GetStatisticsAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var distribution = await DbSet
            .Where(r => r.BusinessId == businessId && r.IsPublished && !r.IsHidden && !r.IsDeleted)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalReviews = distribution.Sum(d => d.Count);
        var avgRating = totalReviews > 0
            ? Math.Round(distribution.Sum(d => d.Rating * d.Count) / (double)totalReviews, 1)
            : 0.0;

        var totalWithReplies = await DbSet
            .CountAsync(r => r.BusinessId == businessId && r.ProviderReply != null && !r.IsDeleted, cancellationToken);

        var totalHidden = await DbSet
            .CountAsync(r => r.BusinessId == businessId && r.IsHidden && !r.IsDeleted, cancellationToken);

        return new ReviewStatisticsResult
        {
            AverageRating = avgRating,
            TotalReviews = totalReviews,
            FiveStarCount = distribution.FirstOrDefault(d => d.Rating == 5)?.Count ?? 0,
            FourStarCount = distribution.FirstOrDefault(d => d.Rating == 4)?.Count ?? 0,
            ThreeStarCount = distribution.FirstOrDefault(d => d.Rating == 3)?.Count ?? 0,
            TwoStarCount = distribution.FirstOrDefault(d => d.Rating == 2)?.Count ?? 0,
            OneStarCount = distribution.FirstOrDefault(d => d.Rating == 1)?.Count ?? 0,
            TotalWithReplies = totalWithReplies,
            TotalHidden = totalHidden
        };
    }

#pragma warning disable CS8602 // Null-forgiving operator not recognized inside GroupBy translation
    public async Task<IReadOnlyList<TopRatedProviderResult>> GetTopRatedProvidersAsync(int count, CancellationToken cancellationToken = default)
    {
        var results = await DbSet
            .Include(r => r.Provider).ThenInclude(p => p.User)
            .Include(r => r.Business)
            .Where(r => r.IsPublished && !r.IsHidden && !r.IsDeleted && r.ProviderId != null)
            .GroupBy(r => r.ProviderId)
            .Select(g => new
            {
                ProviderId = g.Key!.Value,
                FirstName = g.First().Provider!.User!.FirstName,
                LastName = g.First().Provider.User.LastName,
                BusinessName = g.First().Business!.Name,
                AverageRating = g.Average(r => (double)r.Rating),
                TotalReviews = g.Count()
            })
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.TotalReviews)
            .Take(count)
            .ToListAsync(cancellationToken);

        return results.Select(r => new TopRatedProviderResult
        {
            ProviderId = r.ProviderId,
            ProviderName = $"{r.FirstName} {r.LastName}",
            BusinessName = r.BusinessName,
            AverageRating = r.AverageRating,
            TotalReviews = r.TotalReviews
        }).ToList();
    }
#pragma warning restore CS8602

    public async Task<bool> HasCustomerVotedAsync(Guid reviewId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var voteCount = await Context.Set<ReviewVote>()
            .CountAsync(v => v.ReviewId == reviewId && v.CustomerId == customerId, cancellationToken);
        return voteCount > 0;
    }

    public async Task AddVoteAsync(ReviewVote vote, CancellationToken cancellationToken = default)
    {
        await Context.Set<ReviewVote>().AddAsync(vote, cancellationToken);
    }

    public async Task AddReportAsync(ReviewReport report, CancellationToken cancellationToken = default)
    {
        await Context.Set<ReviewReport>().AddAsync(report, cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewReport>> GetReportsAsync(ReportStatus? statusFilter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<ReviewReport>()
            .Include(r => r.Review)
            .Include(r => r.ReportedBy)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(r => r.Status == statusFilter.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
