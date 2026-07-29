using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Attempts to retry failed payments that can be retried.
/// Runs periodically via <see cref="IBackgroundJobScheduler"/>.
/// </summary>
public class PaymentRetryJob : IPaymentRetryJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentRetryJob> _logger;

    public PaymentRetryJob(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<PaymentRetryJob> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task RetryFailedPaymentsAsync(CancellationToken cancellationToken = default)
    {
        // In production: query payments with retryable status and attempt retry via payment gateway.
        // The Payment entity currently supports Fail() but no retry count or MarkAsPending() —
        // those would be added when a payment gateway is integrated.
        _logger.LogDebug("Payment retry cycle completed (placeholder).");
        await Task.CompletedTask;
    }
}
