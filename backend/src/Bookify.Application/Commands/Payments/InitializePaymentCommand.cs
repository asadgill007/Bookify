using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Payments;

public sealed record InitializePaymentCommand : IRequest<Result<PaymentResult>>
{
    public Guid AppointmentId { get; init; }
    public Guid CustomerId { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string? ReturnUrl { get; init; }
}

public sealed class InitializePaymentCommandValidator : AbstractValidator<InitializePaymentCommand>
{
    public InitializePaymentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage("Payment method is required.");
    }
}

public sealed class InitializePaymentCommandHandler : IRequestHandler<InitializePaymentCommand, Result<PaymentResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<InitializePaymentCommandHandler> _logger;

    public InitializePaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<InitializePaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Result<PaymentResult>> Handle(InitializePaymentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result<PaymentResult>.Failure("Appointment not found.", "NOT_FOUND");

        if (appointment.CustomerId != request.CustomerId)
            return Result<PaymentResult>.Failure("You can only pay for your own appointments.", "FORBIDDEN");

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
            return Result<PaymentResult>.Failure("Invalid payment method.", "INVALID_PAYMENT_METHOD");

        // Check if payment already exists
        var existingPayment = await _unitOfWork.Payments.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        if (existingPayment != null)
            return Result<PaymentResult>.Failure("Payment already exists for this appointment.", "PAYMENT_EXISTS");

        var payment = new Payment(
            request.AppointmentId,
            request.CustomerId,
            appointment.TotalAmount,
            appointment.Currency,
            paymentMethod);

        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var initRequest = new PaymentInitializeRequest
        {
            AppointmentId = request.AppointmentId,
            Amount = appointment.TotalAmount,
            Currency = appointment.Currency,
            PaymentMethod = request.PaymentMethod,
            ReturnUrl = request.ReturnUrl,
            CustomerEmail = appointment.Customer?.Email,
            CustomerName = appointment.Customer?.FullName
        };

        var paymentResult = await _paymentService.InitializePaymentAsync(initRequest, cancellationToken);

        if (!paymentResult.IsSuccess)
        {
            payment.Fail();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<PaymentResult>.Failure(paymentResult.ErrorMessage ?? "Payment initialization failed.", "PAYMENT_FAILED");
        }

        payment.Authorize(paymentResult.TransactionId ?? Guid.NewGuid().ToString("N"));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment initialized for appointment {AppointmentId}: {Amount} {Currency} via {Method}",
            request.AppointmentId, appointment.TotalAmount, appointment.Currency, request.PaymentMethod);

        return Result<PaymentResult>.Success(new PaymentResult
        {
            TransactionId = paymentResult.TransactionId,
            PaymentUrl = paymentResult.PaymentUrl,
            Status = "Authorized"
        });
    }
}

public sealed record ConfirmPaymentCommand : IRequest<Result>
{
    public string TransactionId { get; init; } = string.Empty;
}

public sealed class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ConfirmPaymentCommandHandler> _logger;

    public ConfirmPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<ConfirmPaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Result> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var confirmResult = await _paymentService.ConfirmPaymentAsync(request.TransactionId, cancellationToken);

        if (!confirmResult.IsSuccess)
            return Result.Failure(confirmResult.ErrorMessage ?? "Payment confirmation failed.", "CONFIRMATION_FAILED");

        _logger.LogInformation("Payment confirmed for transaction {TransactionId}", request.TransactionId);
        return Result.Success();
    }
}

public sealed record RefundPaymentCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
}

public sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Refund amount must be greater than 0.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Result> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment == null)
            return Result.Failure("Payment not found.", "NOT_FOUND");

        var refundRequest = new RefundRequest
        {
            TransactionId = payment.TransactionId ?? string.Empty,
            Amount = request.Amount,
            Reason = request.Reason
        };

        var refundResult = await _paymentService.RefundPaymentAsync(refundRequest, cancellationToken);

        if (!refundResult.IsSuccess)
            return Result.Failure(refundResult.ErrorMessage ?? "Refund failed.", "REFUND_FAILED");

        payment.Refund(request.Amount, request.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment {PaymentId} refunded: {Amount} by Admin {AdminId}. Reason: {Reason}",
            request.PaymentId, request.Amount, request.AdminUserId, request.Reason ?? "N/A");

        return Result.Success();
    }
}

public class PaymentResult
{
    public string? TransactionId { get; set; }
    public string? PaymentUrl { get; set; }
    public string Status { get; set; } = "Pending";
}
