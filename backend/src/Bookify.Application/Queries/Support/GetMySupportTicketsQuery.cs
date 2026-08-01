using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Support;

/// <summary>
/// List the current user's support tickets (newest first).
/// </summary>
public sealed record GetMySupportTicketsQuery : IRequest<Result<IReadOnlyList<SupportTicketDto>>>
{
    public Guid UserId { get; init; }
}

public sealed class SupportTicketDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class GetMySupportTicketsQueryHandler : IRequestHandler<GetMySupportTicketsQuery, Result<IReadOnlyList<SupportTicketDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMySupportTicketsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<IReadOnlyList<SupportTicketDto>>> Handle(GetMySupportTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _unitOfWork.SupportTickets.GetByUserAsync(request.UserId, cancellationToken);

        var dtos = tickets.Select(t => new SupportTicketDto
        {
            Id = t.Id,
            Category = t.Category,
            Subject = t.Subject,
            Message = t.Message,
            AppointmentId = t.AppointmentId,
            Status = t.Status.ToString(),
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result<IReadOnlyList<SupportTicketDto>>.Success(dtos);
    }
}
