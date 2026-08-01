using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Support;

/// <summary>
/// Create a support ticket from the Contact Support form or the
/// Report-a-Problem flow (optionally linked to an appointment).
/// </summary>
public sealed record CreateSupportTicketCommand : IRequest<Result<SupportTicketResult>>
{
    public Guid UserId { get; init; }
    public string Category { get; init; } = "General";
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Guid? AppointmentId { get; init; }
    public string? ContactEmail { get; init; }
}

public sealed class CreateSupportTicketCommandValidator : AbstractValidator<CreateSupportTicketCommand>
{
    public CreateSupportTicketCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}

public sealed class SupportTicketResult
{
    public Guid TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, Result<SupportTicketResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSupportTicketCommandHandler> _logger;

    public CreateSupportTicketCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateSupportTicketCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SupportTicketResult>> Handle(CreateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        if (request.AppointmentId.HasValue)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId.Value, cancellationToken);
            if (appointment == null)
                return Result<SupportTicketResult>.Failure("Appointment not found.", "NOT_FOUND");
            if (appointment.CustomerId != request.UserId)
                return Result<SupportTicketResult>.Failure("Access denied.", "FORBIDDEN");
        }

        var ticket = new SupportTicket(
            request.UserId,
            request.Category,
            request.Subject,
            request.Message,
            request.AppointmentId,
            request.ContactEmail);

        await _unitOfWork.SupportTickets.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {TicketId} created by user {UserId} (category {Category})",
            ticket.Id, request.UserId, request.Category);

        return Result<SupportTicketResult>.Success(new SupportTicketResult
        {
            TicketId = ticket.Id,
            Status = ticket.Status.ToString()
        });
    }
}
