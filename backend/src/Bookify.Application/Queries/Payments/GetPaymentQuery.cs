using Bookify.Application.Common;
using Bookify.Application.DTOs.Payments;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Payments;

public sealed record GetPaymentQuery : IRequest<Result<PaymentDto>>
{
    public Guid PaymentId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, Result<PaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaymentDto>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found.", "NOT_FOUND");

        if (payment.CustomerId != request.UserId)
            return Result<PaymentDto>.Failure("Access denied.", "FORBIDDEN");

        return Result<PaymentDto>.Success(new PaymentDto
        {
            Id = payment.Id,
            AppointmentId = payment.AppointmentId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentMethod = payment.PaymentMethod.ToString(),
            Status = payment.Status.ToString(),
            TransactionId = payment.TransactionId,
            IsDeposit = payment.IsDeposit,
            RefundAmount = payment.RefundAmount,
            CreatedAt = payment.CreatedAt
        });
    }
}

public sealed record GetPaymentHistoryQuery : IRequest<Result<PaginatedList<PaymentDto>>>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, Result<PaginatedList<PaymentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentHistoryQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<PaymentDto>>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        var payments = await _unitOfWork.Payments.GetByCustomerIdAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Payments.GetCustomerPaymentCountAsync(request.UserId, cancellationToken);

        var items = payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            AppointmentId = p.AppointmentId,
            Amount = p.Amount,
            Currency = p.Currency,
            PaymentMethod = p.PaymentMethod.ToString(),
            Status = p.Status.ToString(),
            TransactionId = p.TransactionId,
            IsDeposit = p.IsDeposit,
            RefundAmount = p.RefundAmount,
            CreatedAt = p.CreatedAt
        }).ToList();

        return Result<PaginatedList<PaymentDto>>.Success(
            new PaginatedList<PaymentDto>(items, request.Page, request.PageSize, total));
    }
}
