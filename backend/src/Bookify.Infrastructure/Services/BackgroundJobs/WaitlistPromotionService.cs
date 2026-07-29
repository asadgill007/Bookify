using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services.BackgroundJobs;

public class WaitlistPromotionService : IWaitlistPromotionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<WaitlistPromotionService> _logger;

    public WaitlistPromotionService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<WaitlistPromotionService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PromotionResult> PromoteNextAsync(
        Guid providerId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken = default)
    {
        var pending = await _unitOfWork.Waitlist.GetPendingEntriesAsync(providerId, date, cancellationToken);
        var candidate = pending.FirstOrDefault();

        if (candidate == null)
        {
            return new PromotionResult { IsPromoted = false, Message = "No waiting customers on the list." };
        }

        candidate.Promote();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.SendNotificationAsync(
            candidate.CustomerId,
            NotificationType.BookingConfirmed,
            "Appointment Slot Available!",
            $"A slot has opened up for {date:d} at {startTime}. Book now before it's taken!",
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Promoted waitlist entry {EntryId} for customer {CustomerId} with provider {ProviderId} on {Date}",
            candidate.Id, candidate.CustomerId, providerId, date);

        return new PromotionResult
        {
            IsPromoted = true,
            EntryId = candidate.Id,
            CustomerId = candidate.CustomerId,
            Message = $"Promoted customer to the top of the waitlist for {date:d}."
        };
    }

    public async Task<int> ExpireOldEntriesAsync(CancellationToken cancellationToken = default)
    {
        var count = await _unitOfWork.Waitlist.ExpireOldEntriesAsync(cancellationToken);

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Expired {Count} old waitlist entries", count);
        }

        return count;
    }

    public async Task<int> GetWaitCountAsync(Guid providerId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var pending = await _unitOfWork.Waitlist.GetPendingEntriesAsync(providerId, date, cancellationToken);
        return pending.Count;
    }
}
