using FluentValidation;

namespace Bookify.Application.DTOs.Payments;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public bool IsDeposit { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InitializePaymentRequest
{
    public Guid AppointmentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class InitializePaymentRequestValidator : AbstractValidator<InitializePaymentRequest>
{
    public InitializePaymentRequestValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.PaymentMethod).NotEmpty();
    }
}
