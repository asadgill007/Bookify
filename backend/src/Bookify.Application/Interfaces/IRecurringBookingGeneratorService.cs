namespace Bookify.Application.Interfaces;

/// <summary>
/// Background service that generates appointment instances from active recurring booking series.
/// Runs periodically to create appointments for upcoming dates based on recurrence rules.
/// </summary>
public interface IRecurringBookingGeneratorService
{
    /// <summary>
    /// Process all active recurring series and generate appointment instances for upcoming dates.
    /// Called periodically by a background job (Hangfire/Quartz).
    /// </summary>
    Task GenerateAppointmentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate appointments for a specific recurring series up to the given end date.
    /// </summary>
    Task GenerateAppointmentsForSeriesAsync(
        Guid recurringBookingId,
        DateTime? upTo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the next N expected dates for a recurring series without creating appointments.
    /// </summary>
    Task<List<DateTime>> GetExpectedDatesAsync(
        Guid recurringBookingId,
        int count = 10,
        CancellationToken cancellationToken = default);
}
