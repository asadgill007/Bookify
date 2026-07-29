using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Centralized business rule validation for the booking domain.
/// Every appointment-related action validates rules here before execution.
/// </summary>
public class BusinessRuleValidator : IBusinessRuleValidator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BusinessRuleValidator> _logger;

    public BusinessRuleValidator(IUnitOfWork unitOfWork, ILogger<BusinessRuleValidator> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> ValidateAppointmentCreationAsync(
        Guid providerId,
        Guid serviceId,
        Guid businessId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        // Rule 1: Service must exist and be active
        var service = await _unitOfWork.Services.GetByIdAsync(serviceId, cancellationToken);
        if (service == null)
            return Result.Failure("Service not found.", "NOT_FOUND");
        if (!service.IsActive)
            return Result.Failure("This service is no longer available.", "SERVICE_INACTIVE");

        // Rule 2: Provider must exist and be active
        var provider = await _unitOfWork.Providers.GetByIdAsync(providerId, cancellationToken);
        if (provider == null)
            return Result.Failure("Provider not found.", "NOT_FOUND");
        if (!provider.IsActive)
            return Result.Failure("This provider is no longer accepting appointments.", "PROVIDER_INACTIVE");

        // Rule 3: Business must exist
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        // Rule 4: Service must belong to the specified business
        if (service.BusinessId != businessId)
            return Result.Failure("Service does not belong to the specified business.", "SERVICE_MISMATCH");

        // Rule 5: Provider must belong to the specified business
        if (provider.BusinessId != businessId)
            return Result.Failure("Provider does not belong to the specified business.", "PROVIDER_MISMATCH");

        // Rule 6: Appointment must be in the future
        if (startTime <= DateTime.UtcNow.AddMinutes(5))
            return Result.Failure("Appointment must be scheduled at least 5 minutes from now.", "TOO_SOON");

        // Rule 7: End time must be after start time
        if (endTime <= startTime)
            return Result.Failure("End time must be after start time.", "INVALID_TIME_RANGE");

        // Rule 8: Duration must match service duration (5-minute tolerance)
        var requestedDuration = (endTime - startTime).TotalMinutes;
        if (Math.Abs(requestedDuration - service.DurationMinutes) > 5)
            return Result.Failure(
                $"Appointment duration ({requestedDuration:F0} min) does not match service duration ({service.DurationMinutes} min).",
                "DURATION_MISMATCH");

        // Rule 9: No overlapping appointments
        var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
            providerId, startTime, endTime, null, cancellationToken);

        if (hasConflict)
            return Result.Failure("The selected time slot is already booked.", "SLOT_CONFLICT");

        return Result.Success();
    }

    public async Task<Result> ValidateAppointmentCancellationAsync(
        Guid appointmentId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
            return Result.Failure("Appointment not found.", "NOT_FOUND");

        // Rule: Cannot cancel completed appointments
        if (appointment.Status == AppointmentStatus.Completed)
            return Result.Failure("Cannot cancel a completed appointment.", "ALREADY_COMPLETED");

        if (appointment.Status == AppointmentStatus.Cancelled)
            return Result.Failure("Appointment is already cancelled.", "ALREADY_CANCELLED");

        return Result.Success();
    }

    public async Task<Result> ValidateSlotAvailabilityAsync(
        Guid providerId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
            providerId, startTime, endTime, excludeAppointmentId, cancellationToken);

        if (hasConflict)
            return Result.Failure("The selected time slot is already booked.", "SLOT_CONFLICT");

        return Result.Success();
    }
}
