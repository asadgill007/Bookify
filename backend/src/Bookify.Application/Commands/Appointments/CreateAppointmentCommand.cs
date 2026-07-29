using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Appointments;

public sealed record CreateAppointmentCommand : IRequest<Result<AppointmentResult>>
{
    public Guid ProviderId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid BusinessId { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string? CustomerNotes { get; init; }
    public Guid CustomerId { get; init; }
}

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Appointment must be scheduled in the future.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");

        RuleFor(x => x.CustomerNotes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public sealed class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;

    public CreateAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<AppointmentResult>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Validate service exists
        var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId, cancellationToken);
        if (service == null)
            return Result<AppointmentResult>.Failure("Service not found.", "NOT_FOUND");
        if (!service.IsActive)
            return Result<AppointmentResult>.Failure("This service is no longer available.", "SERVICE_INACTIVE");

        // Validate provider exists and is active
        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null)
            return Result<AppointmentResult>.Failure("Provider not found.", "NOT_FOUND");
        if (!provider.IsActive)
            return Result<AppointmentResult>.Failure("This provider is no longer available.", "PROVIDER_INACTIVE");

        // Validate business exists
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result<AppointmentResult>.Failure("Business not found.", "NOT_FOUND");

        // Validate the service belongs to the specified business
        if (service.BusinessId != request.BusinessId)
            return Result<AppointmentResult>.Failure("Service does not belong to the specified business.", "SERVICE_MISMATCH");

        // Validate the provider belongs to the specified business
        if (provider.BusinessId != request.BusinessId)
            return Result<AppointmentResult>.Failure("Provider does not belong to the specified business.", "PROVIDER_MISMATCH");

        // Check for double booking (overlapping appointments)
        var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
            request.ProviderId, request.StartTime, request.EndTime, null, cancellationToken);

        if (hasConflict)
            return Result<AppointmentResult>.Failure("The selected time slot is already booked.", "SLOT_CONFLICT");

        // Validate appointment duration matches service duration
        var requestedDuration = (request.EndTime - request.StartTime).TotalMinutes;
        if (Math.Abs(requestedDuration - service.DurationMinutes) > 5) // 5-minute tolerance
            return Result<AppointmentResult>.Failure(
                $"Appointment duration ({requestedDuration:F0} min) does not match service duration ({service.DurationMinutes} min).",
                "DURATION_MISMATCH");

        var bookingRef = GenerateBookingReference();
        var totalAmount = service.PriceAmount;

        var appointment = new Appointment(
            bookingRef,
            request.CustomerId,
            request.ProviderId,
            request.ServiceId,
            request.BusinessId,
            request.StartTime,
            request.EndTime,
            totalAmount,
            service.PriceCurrency);

        if (!string.IsNullOrWhiteSpace(request.CustomerNotes))
            appointment.SetNotes(request.CustomerNotes);

        await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Appointment {BookingReference} created: Customer={CustomerId}, Provider={ProviderId}, Service={ServiceId}, " +
            "Start={StartTime}, End={EndTime}, Amount={Amount} {Currency}",
            bookingRef, request.CustomerId, request.ProviderId, request.ServiceId,
            request.StartTime, request.EndTime, totalAmount, service.PriceCurrency);

        // Send notification to the customer
        await _notificationService.SendNotificationAsync(
            request.CustomerId,
            Domain.Enums.NotificationType.BookingConfirmed,
            "Appointment Created",
            $"Your appointment ({bookingRef}) has been created successfully.",
            System.Text.Json.JsonSerializer.Serialize(new { appointmentId = appointment.Id }),
            cancellationToken);

        return Result<AppointmentResult>.Success(new AppointmentResult
        {
            Id = appointment.Id,
            BookingReference = appointment.BookingReference,
            Status = appointment.Status.ToString(),
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            TotalAmount = appointment.TotalAmount,
            Currency = appointment.Currency,
            ServiceName = service.Name
        });
    }

    private static string GenerateBookingReference()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = chars[Random.Shared.Next(chars.Length)];

        return $"BOK-{new string(code)}";
    }
}

public class AppointmentResult
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string ServiceName { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
}
