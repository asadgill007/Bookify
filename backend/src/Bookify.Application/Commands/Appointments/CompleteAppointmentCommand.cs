using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Appointments;

public sealed record CompleteAppointmentCommand : IRequest<Result>
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class CompleteAppointmentCommandValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CompleteAppointmentCommandHandler> _logger;

    public CompleteAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<CompleteAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result.Failure("Appointment not found.", "NOT_FOUND");

        appointment.Complete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Appointment {BookingReference} completed by {UserId}",
            appointment.BookingReference, request.UserId);

        // Notify the customer
        await _notificationService.SendNotificationAsync(
            appointment.CustomerId,
            NotificationType.BookingConfirmed,
            "Appointment Completed",
            $"Your appointment ({appointment.BookingReference}) has been marked as completed. Please leave a review!",
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}

public sealed record RescheduleAppointmentCommand : IRequest<Result<AppointmentResult>>
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
    public DateTime NewStartTime { get; init; }
    public DateTime NewEndTime { get; init; }
    public string? Reason { get; init; }
}

public sealed class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.NewStartTime)
            .NotEmpty().WithMessage("New start time is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("New appointment time must be in the future.");

        RuleFor(x => x.NewEndTime)
            .NotEmpty().WithMessage("New end time is required.")
            .GreaterThan(x => x.NewStartTime).WithMessage("New end time must be after new start time.");
    }
}

public sealed class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, Result<AppointmentResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RescheduleAppointmentCommandHandler> _logger;

    public RescheduleAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<RescheduleAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<AppointmentResult>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result<AppointmentResult>.Failure("Appointment not found.", "NOT_FOUND");

        // Verify authorization
        if (appointment.CustomerId != request.UserId &&
            appointment.Provider?.UserId != request.UserId)
        {
            return Result<AppointmentResult>.Failure("You are not authorized to reschedule this appointment.", "FORBIDDEN");
        }

        // Check for conflicts with the new time
        var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
            appointment.ProviderId, request.NewStartTime, request.NewEndTime, appointment.Id, cancellationToken);

        if (hasConflict)
            return Result<AppointmentResult>.Failure("The requested time slot is already booked.", "SLOT_CONFLICT");

        // Create the rescheduled appointment (domain entity handles the chain)
        var rescheduledAppointment = appointment.Reschedule(request.NewStartTime, request.NewEndTime);
        await _unitOfWork.Appointments.AddAsync(rescheduledAppointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Appointment {BookingReference} rescheduled by {UserId}. New time: {NewStart}-{NewEnd}",
            appointment.BookingReference, request.UserId, request.NewStartTime, request.NewEndTime);

        // Notify both parties
        var notifyUserId = appointment.CustomerId == request.UserId
            ? (appointment.Provider?.UserId ?? Guid.Empty)
            : appointment.CustomerId;

        if (notifyUserId != Guid.Empty)
        {
            await _notificationService.SendNotificationAsync(
                notifyUserId,
                NotificationType.BookingConfirmed,
                "Appointment Rescheduled",
                $"Appointment {appointment.BookingReference} has been rescheduled to {request.NewStartTime:F}.",
                cancellationToken: cancellationToken);
        }

        var service = await _unitOfWork.Services.GetByIdAsync(appointment.ServiceId, cancellationToken);

        return Result<AppointmentResult>.Success(new AppointmentResult
        {
            Id = rescheduledAppointment.Id,
            BookingReference = rescheduledAppointment.BookingReference,
            Status = rescheduledAppointment.Status.ToString(),
            StartTime = rescheduledAppointment.StartTime,
            EndTime = rescheduledAppointment.EndTime,
            TotalAmount = rescheduledAppointment.TotalAmount,
            Currency = rescheduledAppointment.Currency,
            ServiceName = service?.Name ?? ""
        });
    }
}
