using Bookify.Domain.Common;
using Bookify.Domain.DomainEvents;
using Bookify.Domain.Enums;

namespace Bookify.Domain.Entities;

public sealed class Payment : BaseEntity
{
    public Guid AppointmentId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? TransactionId { get; private set; }
    public bool IsDeposit { get; private set; }
    public decimal? RefundAmount { get; private set; }
    public string? RefundReason { get; private set; }

    public Appointment Appointment { get; private set; } = null!;
    public User Customer { get; private set; } = null!;
    public ICollection<PaymentTransaction> Transactions { get; private set; } = new List<PaymentTransaction>();

    private Payment() { }

    public Payment(Guid appointmentId, Guid customerId, decimal amount, string currency, PaymentMethod paymentMethod)
    {
        AppointmentId = appointmentId;
        CustomerId = customerId;
        Amount = Math.Round(amount, 2);
        Currency = currency;
        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Pending;
    }

    public void Authorize(string transactionId)
    {
        TransactionId = transactionId;
        Status = PaymentStatus.Authorized;
        Transactions.Add(new PaymentTransaction(Id, "Authorization", Amount, transactionId));
        Touch();
    }

    public void Capture(string? transactionId = null)
    {
        if (Status != PaymentStatus.Authorized)
            throw new InvalidOperationException($"Cannot capture payment in {Status} status.");

        Status = PaymentStatus.Captured;
        Transactions.Add(new PaymentTransaction(Id, "Capture", Amount, transactionId));
        AddDomainEvent(new PaymentCapturedEvent(Id, AppointmentId, CustomerId, Amount, Currency, DateTime.UtcNow));
        Touch();
    }

    public void Refund(decimal amount, string? reason = null)
    {
        if (Status != PaymentStatus.Captured)
            throw new InvalidOperationException($"Cannot refund payment in {Status} status.");

        if (amount <= 0 || amount > Amount)
            throw new ArgumentException("Invalid refund amount.", nameof(amount));

        RefundAmount = Math.Round(amount, 2);
        RefundReason = reason?.Trim();
        Status = amount == Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        Transactions.Add(new PaymentTransaction(Id, "Refund", amount));
        Touch();
    }

    public void Fail()
    {
        Status = PaymentStatus.Failed;
        Touch();
    }
}

public sealed class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public string Action { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string? ProviderResponse { get; private set; }
    public bool IsSuccess { get; private set; }

    public Payment Payment { get; private set; } = null!;

    private PaymentTransaction() { }

    public PaymentTransaction(Guid paymentId, string action, decimal amount, string? providerResponse = null)
    {
        PaymentId = paymentId;
        Action = action;
        Amount = Math.Round(amount, 2);
        ProviderResponse = providerResponse;
        IsSuccess = true;
    }
}
