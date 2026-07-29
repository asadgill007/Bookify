using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Processes pending SMS notifications from the queue.
/// Runs periodically via <see cref="IBackgroundJobScheduler"/>.
/// </summary>
public class SmsQueueJob : ISmsQueueJob
{
    private readonly ISmsService _smsService;
    private readonly ILogger<SmsQueueJob> _logger;

    public SmsQueueJob(ISmsService smsService, ILogger<SmsQueueJob> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ProcessSmsQueueAsync(CancellationToken cancellationToken = default)
    {
        // In production: query pending SMS notifications and send via ISmsService.
        _logger.LogDebug("SMS queue processing cycle completed (placeholder).");
        await Task.CompletedTask;
    }
}
