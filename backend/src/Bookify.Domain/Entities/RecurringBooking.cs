using Bookify.Domain.Common;
using Bookify.Domain.Enums;

namespace Bookify.Domain.Entities;

public enum RecurrenceType
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Custom = 3
}

public sealed class RecurringBooking : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid BusinessId { get; private set; }

    public RecurrenceType RecurrenceType { get; private set; }
    public int Interval { get; private set; } = 1;
    public int? DayOfMonth { get; private set; }
    public List<DayOfWeek> DaysOfWeek { get; private set; } = new();
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public DateTime SeriesStartDate { get; private set; }
    public DateTime? SeriesEndDate { get; private set; }
    public int? MaxOccurrences { get; private set; }
    public int OccurrencesCreated { get; private set; }

    public bool IsActive { get; private set; } = true;
    public DateTime? PausedUntil { get; private set; }
    public string? Notes { get; private set; }

    public User Customer { get; private set; } = null!;
    public Provider Provider { get; private set; } = null!;
    public Service Service { get; private set; } = null!;
    public Business Business { get; private set; } = null!;

    private RecurringBooking() { }

    public RecurringBooking(
        Guid customerId,
        Guid providerId,
        Guid serviceId,
        Guid businessId,
        RecurrenceType recurrenceType,
        TimeOnly startTime,
        TimeOnly endTime,
        DateTime seriesStartDate,
        DateTime? seriesEndDate = null,
        int? maxOccurrences = null,
        int interval = 1,
        int? dayOfMonth = null,
        List<DayOfWeek>? daysOfWeek = null,
        string? notes = null)
    {
        CustomerId = customerId;
        ProviderId = providerId;
        ServiceId = serviceId;
        BusinessId = businessId;
        RecurrenceType = recurrenceType;
        StartTime = startTime;
        EndTime = endTime;
        SeriesStartDate = seriesStartDate;
        SeriesEndDate = seriesEndDate;
        MaxOccurrences = maxOccurrences;
        Interval = Math.Max(1, interval);
        DayOfMonth = dayOfMonth;
        DaysOfWeek = daysOfWeek ?? new List<DayOfWeek>();
        Notes = notes?.Trim();
        IsActive = true;

        ValidateRecurrence();
    }

    private void ValidateRecurrence()
    {
        if (StartTime >= EndTime)
            throw new ArgumentException("Start time must be before end time.");

        if (SeriesEndDate.HasValue && SeriesEndDate <= SeriesStartDate)
            throw new ArgumentException("Series end date must be after start date.");

        if (MaxOccurrences.HasValue && MaxOccurrences < 1)
            throw new ArgumentException("Max occurrences must be at least 1.");

        if (RecurrenceType == RecurrenceType.Monthly && !DayOfMonth.HasValue)
            throw new ArgumentException("Day of month is required for monthly recurrence.");

        if (RecurrenceType == RecurrenceType.Weekly && DaysOfWeek.Count == 0)
            throw new ArgumentException("At least one day of week is required for weekly recurrence.");
    }

    public void CancelSeries()
    {
        IsActive = false;
        Touch();
    }

    public void PauseSeries(DateTime until)
    {
        if (until <= DateTime.UtcNow)
            throw new ArgumentException("Pause date must be in the future.");

        PausedUntil = until;
        Touch();
    }

    public void ResumeSeries()
    {
        PausedUntil = null;
        Touch();
    }

    public void IncrementOccurrencesCreated()
    {
        OccurrencesCreated++;
        Touch();
    }

    public void UpdateSchedule(
        TimeOnly startTime,
        TimeOnly endTime,
        DateTime? seriesEndDate,
        int? maxOccurrences,
        string? notes)
    {
        StartTime = startTime;
        EndTime = endTime;
        SeriesEndDate = seriesEndDate;
        MaxOccurrences = maxOccurrences;
        Notes = notes?.Trim();
        Touch();
    }

    public bool HasCompleted => MaxOccurrences.HasValue && OccurrencesCreated >= MaxOccurrences.Value;

    public bool IsPaused => PausedUntil.HasValue && PausedUntil.Value > DateTime.UtcNow;
}
