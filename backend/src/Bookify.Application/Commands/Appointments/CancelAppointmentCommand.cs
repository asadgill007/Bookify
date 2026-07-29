using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Appointments;

public sealed record CancelAppointmentCommand : IRequest<Result>
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
    public string? Reason { get; init; }
}

public sealed class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CancelAppointmentCommandHandler> _logger;

    public CancelAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<CancelAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result.Failure("Appointment not found.", "NOT_FOUND");

        // Validate the user is authorized to cancel
        if (appointment.CustomerId != request.UserId &&
            appointment.Provider?.UserId != request.UserId)
        {
            return Result.Failure("You are not authorized to cancel this appointment.", "FORBIDDEN");
        }

        // Cannot cancel completed appointments
        if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            return Result.Failure($"Cannot cancel appointment in {appointment.Status} status.", "INVALID_STATUS");

        appointment.Cancel(request.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Appointment {BookingReference} cancelled by {UserId}. Reason: {Reason}",
            appointment.BookingReference, request.UserId, request.Reason ?? "Not specified");

        // Notify the other party
        var notifyUserId = appointment.CustomerId == request.UserId
            ? (appointment.Provider?.UserId ?? Guid.Empty)
            : appointment.CustomerId;

        if (notifyUserId != Guid.Empty)
        {
            await _notificationService.SendNotificationAsync(
                notifyUserId,
                NotificationType.BookingCancelled,
                "Appointment Cancelled",
                $"Appointment {appointment.BookingReference} has been cancelled." +
                (!string.IsNullOrEmpty(request.Reason) ? $" Reason: {request.Reason}" : ""),
                cancellationToken: cancellationToken);
        }

        return Result.Success();
    }
}

public sealed record ConfirmAppointmentCommand : IRequest<Result>
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class ConfirmAppointmentCommandValidator : AbstractValidator<ConfirmAppointmentCommand>
{
    public ConfirmAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class ConfirmAppointmentCommandHandler : IRequestHandler<ConfirmAppointmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ConfirmAppointmentCommandHandler> _logger;

    public ConfirmAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ConfirmAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result.Failure("Appointment not found.", "NOT_FOUND");

        appointment.Confirm();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Appointment {BookingReference} confirmed by {UserId}",
            appointment.BookingReference, request.UserId);

        // Notify the customer
        await _notificationService.SendNotificationAsync(
            appointment.CustomerId,
            NotificationType.BookingConfirmed,
            "Appointment Confirmed",
            $"Your appointment ({appointment.BookingReference}) has been confirmed.",
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
