using Bookify.Application.Interfaces;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Mock payment service implementation.
/// Replace with StripePaymentService, PayPalPaymentService, JazzCashPaymentService, etc.
/// when integrating actual payment gateways.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly INotificationService _notificationService;

    public PaymentService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task<PaymentResult> InitializePaymentAsync(PaymentInitializeRequest request, CancellationToken cancellationToken = default)
    {
        var result = new PaymentResult
        {
            IsSuccess = true,
            TransactionId = Guid.NewGuid().ToString("N"),
            PaymentUrl = "https://bookify.com/payments/mock"
        };

        return Task.FromResult(result);
    }

    public Task<PaymentResult> ConfirmPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var result = new PaymentResult
        {
            IsSuccess = true,
            TransactionId = transactionId
        };

        return Task.FromResult(result);
    }

    public Task<PaymentResult> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default)
    {
        var result = new PaymentResult
        {
            IsSuccess = true,
            TransactionId = request.TransactionId
        };

        return Task.FromResult(result);
    }

    public Task<PaymentResult> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var result = new PaymentResult
        {
            IsSuccess = true,
            TransactionId = transactionId
        };

        return Task.FromResult(result);
    }
}
