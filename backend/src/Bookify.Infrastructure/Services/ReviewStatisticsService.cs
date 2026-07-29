using Bookify.Application.Interfaces;
using Bookify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

public class ReviewStatisticsService : IReviewStatisticsService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReviewStatisticsService> _logger;

    public ReviewStatisticsService(AppDbContext context, ILogger<ReviewStatisticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RecalculateBusinessRatingAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var stats = await _context.Reviews
            .Where(r => r.BusinessId == businessId && r.IsPublished && !r.IsHidden && !r.IsDeleted)
            .GroupBy(r => 1)
            .Select(g => new
            {
                AverageRating = g.Average(r => (double)r.Rating),
                TotalReviews = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var business = await _context.Businesses.FindAsync(new object[] { businessId }, cancellationToken);
        if (business == null)
        {
            _logger.LogWarning("Business {BusinessId} not found for rating recalculation.", businessId);
            return;
        }

        var avgRating = stats != null ? Math.Round(stats.AverageRating, 1) : 0.0;
        var totalReviews = stats?.TotalReviews ?? 0;

        business.UpdateRating(avgRating, totalReviews);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Recalculated rating for business {BusinessId}: {AverageRating} ({TotalReviews} reviews)",
            businessId, avgRating, totalReviews);
    }
}
