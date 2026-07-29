using Bookify.Application.Common;

namespace Bookify.Application.Interfaces;

public class PaymentInitializeRequest
{
    public Guid AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RefundRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Abstract payment service. No gateway is integrated yet.
/// Implementations for Stripe, PayPal, JazzCash, EasyPaisa, etc.
/// can be plugged in without changing business logic.
/// </summary>
public interface IPaymentService
{
    Task<PaymentResult> InitializePaymentAsync(PaymentInitializeRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResult> ConfirmPaymentAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<PaymentResult> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResult> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default);
}
