namespace Bookify.Application.Interfaces;

/// <summary>
/// Processes appointment reminders (e.g., 24h before, 1h before).
/// Called by <see cref="IBackgroundJobScheduler"/> on a recurring schedule.
/// </summary>
public interface IReminderJob
{
    /// <summary>Send appointment reminder notifications for upcoming appointments.</summary>
    Task ProcessAppointmentRemindersAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Performs periodic cleanup of expired, stale, or soft-deleted data.
/// Called by <see cref="IBackgroundJobScheduler"/> on a daily schedule.
/// </summary>
public interface ICleanupJob
{
    /// <summary>Clean up expired refresh tokens.</summary>
    Task CleanExpiredRefreshTokensAsync(CancellationToken cancellationToken = default);

    /// <summary>Clean up soft-deleted records older than the retention period.</summary>
    Task CleanSoftDeletedRecordsAsync(CancellationToken cancellationToken = default);

    /// <summary>Expire old waitlist entries past their expiry date.</summary>
    Task ExpireWaitlistEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Remove old appointment logs beyond the retention period.</summary>
    Task CleanOldAppointmentLogsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Processes the email notification queue.
/// Called by <see cref="IBackgroundJobScheduler"/> on a recurring schedule.
/// </summary>
public interface IEmailQueueJob
{
    /// <summary>Process pending emails from the notification queue.</summary>
    Task ProcessEmailQueueAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Processes the SMS notification queue.
/// Called by <see cref="IBackgroundJobScheduler"/> on a recurring schedule.
/// </summary>
public interface ISmsQueueJob
{
    /// <summary>Process pending SMS messages from the notification queue.</summary>
    Task ProcessSmsQueueAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Retries failed payments that can be retried.
/// Called by <see cref="IBackgroundJobScheduler"/> on a recurring schedule.
/// </summary>
public interface IPaymentRetryJob
{
    /// <summary>Attempt to retry failed payments with retryable status.</summary>
    Task RetryFailedPaymentsAsync(CancellationToken cancellationToken = default);
}
