using Bookify.Domain.Common;

namespace Bookify.Domain.ValueObjects;

public sealed class TimeRange : ValueObject
{
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    private TimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public static TimeRange Create(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time.", nameof(startTime));

        return new TimeRange(startTime, endTime);
    }

    public TimeSpan Duration => EndTime - StartTime;

    public bool OverlapsWith(TimeRange other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public bool Contains(TimeOnly time)
    {
        return time >= StartTime && time <= EndTime;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
    }

    public override string ToString() => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
}
