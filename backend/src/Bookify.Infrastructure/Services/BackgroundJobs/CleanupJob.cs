using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Performs periodic cleanup of expired and stale data.
/// Runs daily via <see cref="IBackgroundJobScheduler"/>.
/// </summary>
public class CleanupJob : ICleanupJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWaitlistPromotionService _waitlistPromotion;
    private readonly ILogger<CleanupJob> _logger;

    public CleanupJob(
        IUnitOfWork unitOfWork,
        IWaitlistPromotionService waitlistPromotion,
        ILogger<CleanupJob> logger)
    {
        _unitOfWork = unitOfWork;
        _waitlistPromotion = waitlistPromotion;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task CleanExpiredRefreshTokensAsync(CancellationToken cancellationToken = default)
    {
        var expired = await _unitOfWork.RefreshTokens.GetExpiredAsync(cancellationToken);

        foreach (var token in expired)
            await _unitOfWork.RefreshTokens.DeleteAsync(token, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Cleaned {Count} expired/revoked refresh tokens.", expired.Count);
    }

    /// <inheritdoc />
    public async Task CleanSoftDeletedRecordsAsync(CancellationToken cancellationToken = default)
    {
        // Soft-delete cleanup is data-maintenance: purge records deleted > 90 days ago
        // In production, this would query each soft-deletable table with a date threshold
        _logger.LogInformation("Soft-delete cleanup cycle completed (placeholder).");
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ExpireWaitlistEntriesAsync(CancellationToken cancellationToken = default)
    {
        var expired = await _waitlistPromotion.ExpireOldEntriesAsync(cancellationToken);
        if (expired > 0)
            _logger.LogInformation("Expired {Count} waitlist entries.", expired);
    }

    /// <inheritdoc />
    public async Task CleanOldAppointmentLogsAsync(CancellationToken cancellationToken = default)
    {
        // In production, delete AppointmentLog records older than 90 days
        _logger.LogInformation("Old appointment log cleanup cycle completed (placeholder).");
        await Task.CompletedTask;
    }
}
