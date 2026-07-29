using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Generates appointment instances from active recurring booking series.
/// This is called by a background job (Hangfire/Quartz) on a periodic schedule.
/// </summary>
public class RecurringBookingGeneratorService : IRecurringBookingGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecurringBookingGeneratorService> _logger;

    public RecurringBookingGeneratorService(
        IUnitOfWork unitOfWork,
        ILogger<RecurringBookingGeneratorService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task GenerateAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var activeSeries = await _unitOfWork.RecurringBookings.GetActiveSeriesAsync(cancellationToken);
        _logger.LogInformation("Processing {Count} active recurring series for appointment generation", activeSeries.Count);

        foreach (var series in activeSeries)
        {
            await GenerateAppointmentsForSeriesAsync(series.Id, upTo: DateTime.UtcNow.AddDays(30), cancellationToken);
        }
    }

    public async Task GenerateAppointmentsForSeriesAsync(
        Guid recurringBookingId,
        DateTime? upTo = null,
        CancellationToken cancellationToken = default)
    {
        var series = await _unitOfWork.RecurringBookings.GetByIdAsync(recurringBookingId, cancellationToken);
        if (series == null)
        {
            _logger.LogWarning("Recurring series {Id} not found for generation", recurringBookingId);
            return;
        }

        if (!series.IsActive || series.IsPaused || series.HasCompleted)
        {
            _logger.LogDebug("Series {Id} is not eligible for generation (active={Active}, paused={Paused}, completed={Completed})",
                recurringBookingId, series.IsActive, series.IsPaused, series.HasCompleted);
            return;
        }

        var endDate = upTo ?? series.SeriesEndDate ?? DateTime.UtcNow.AddMonths(3);
        var expectedDates = CalculateExpectedDates(series, endDate);
        var generatedCount = 0;

        foreach (var date in expectedDates)
        {
            try
            {
                if (series.MaxOccurrences.HasValue && series.OccurrencesCreated >= series.MaxOccurrences.Value)
                    break;

                var startDateTime = date.Date.Add(series.StartTime.ToTimeSpan());
                var endDateTime = date.Date.Add(series.EndTime.ToTimeSpan());

                // Skip if start time is in the past
                if (startDateTime <= DateTime.UtcNow)
                    continue;

                // Check for conflicts with existing appointments
                var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
                    series.ProviderId, startDateTime, endDateTime, null, cancellationToken);

                if (hasConflict)
                {
                    _logger.LogWarning("Conflict detected for series {Id} on {Date}: slot already booked", recurringBookingId, date);
                    continue;
                }

                var bookingRef = GenerateBookingReference();

                var appointment = new Appointment(
                    bookingRef,
                    series.CustomerId,
                    series.ProviderId,
                    series.ServiceId,
                    series.BusinessId,
                    startDateTime,
                    endDateTime,
                    series.Service?.PriceAmount ?? 0,
                    series.Service?.PriceCurrency ?? "USD");

                await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
                series.IncrementOccurrencesCreated();
                generatedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate appointment for series {Id} on {Date}", recurringBookingId, date);
                // Continue with next date to avoid failing the entire batch
            }
        }

        if (generatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Generated {Count} appointments for recurring series {Id}", generatedCount, recurringBookingId);
        }
    }

    public async Task<List<DateTime>> GetExpectedDatesAsync(
        Guid recurringBookingId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var series = await _unitOfWork.RecurringBookings.GetByIdAsync(recurringBookingId, cancellationToken);
        if (series == null) return new List<DateTime>();

        var endDate = series.SeriesEndDate ?? DateTime.UtcNow.AddMonths(6);
        var allDates = CalculateExpectedDates(series, endDate);
        return allDates.Take(count).ToList();
    }

    private List<DateTime> CalculateExpectedDates(RecurringBooking series, DateTime endDate)
    {
        var dates = new List<DateTime>();
        var current = series.SeriesStartDate;

        while (current <= endDate)
        {
            if (!series.MaxOccurrences.HasValue || dates.Count < series.MaxOccurrences.Value)
            {
                bool shouldInclude = series.RecurrenceType switch
                {
                    RecurrenceType.Daily => true,
                    RecurrenceType.Weekly => series.DaysOfWeek.Contains(current.DayOfWeek),
                    RecurrenceType.Monthly => series.DayOfMonth.HasValue && current.Day == series.DayOfMonth.Value,
                    RecurrenceType.Custom => true,
                    _ => false
                };

                if (shouldInclude && current >= series.SeriesStartDate)
                    dates.Add(current);
            }

            current = series.RecurrenceType switch
            {
                RecurrenceType.Daily => current.AddDays(series.Interval),
                RecurrenceType.Weekly => current.AddDays(1),
                RecurrenceType.Monthly => current.AddMonths(series.Interval),
                RecurrenceType.Custom => current.AddDays(series.Interval),
                _ => current.AddDays(1)
            };
        }

        return dates;
    }

    private static string GenerateBookingReference()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = chars[Random.Shared.Next(chars.Length)];
        return $"REC-{new string(code)}";
    }
}
