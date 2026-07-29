namespace Bookify.Application.Interfaces;

/// <summary>
/// Service for recalculating business review statistics (average rating, star distribution).
/// Should be triggered whenever a review is created, updated, deleted, hidden, or restored.
/// </summary>
public interface IReviewStatisticsService
{
    /// <summary>Recalculate and update the average rating and total reviews for a business.</summary>
    Task RecalculateBusinessRatingAsync(Guid businessId, CancellationToken cancellationToken = default);
}
