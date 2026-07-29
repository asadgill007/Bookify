using Bookify.Application.Common;

namespace Bookify.Application.Interfaces;

/// <summary>
/// Centralized service for validating business rules across the booking domain.
/// All rules are checked here to ensure consistency across all entry points.
/// </summary>
public interface IBusinessRuleValidator
{
    Task<Result> ValidateAppointmentCreationAsync(
        Guid providerId,
        Guid serviceId,
        Guid businessId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);

    Task<Result> ValidateAppointmentCancellationAsync(
        Guid appointmentId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> ValidateSlotAvailabilityAsync(
        Guid providerId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default);
}
