using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Appointments;

/// <summary>
/// Marks an appointment as InProgress (required before completing).
/// </summary>
public sealed record StartAppointmentCommand : IRequest<Result>
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class StartAppointmentCommandValidator : AbstractValidator<StartAppointmentCommand>
{
    public StartAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class StartAppointmentCommandHandler : IRequestHandler<StartAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartAppointmentCommandHandler> _logger;

    public StartAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<StartAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(StartAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result.Failure("Appointment not found.", "NOT_FOUND");

        appointment.Start();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Appointment {BookingReference} started (InProgress) by {UserId}",
            appointment.BookingReference, request.UserId);

        return Result.Success();
    }
}
