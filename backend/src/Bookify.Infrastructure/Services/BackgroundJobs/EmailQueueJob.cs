using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Processes pending email notifications from the queue.
/// Runs periodically via <see cref="IBackgroundJobScheduler"/>.
/// </summary>
public class EmailQueueJob : IEmailQueueJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailQueueJob> _logger;

    public EmailQueueJob(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<EmailQueueJob> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ProcessEmailQueueAsync(CancellationToken cancellationToken = default)
    {
        // In production: query notifications with Type=Email that are pending
        // and send them via IEmailService. For now, this is a placeholder
        // that will be wired when the email queue infrastructure is built.
        _logger.LogDebug("Email queue processing cycle completed (placeholder).");
        await Task.CompletedTask;
    }
}
