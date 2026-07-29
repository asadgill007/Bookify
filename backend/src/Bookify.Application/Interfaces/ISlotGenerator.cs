namespace Bookify.Application.Interfaces;

/// <summary>
/// Generates available time slots for a provider on a given date,
/// considering business hours, breaks, holidays, buffer times, and existing bookings.
/// </summary>
public interface ISlotGenerator
{
    Task<List<TimeSlot>> GenerateSlotsAsync(
        SlotGenerationRequest request,
        CancellationToken cancellationToken = default);
}

public class SlotGenerationRequest
{
    public Guid BusinessId { get; init; }
    public Guid ProviderId { get; init; }
    public Guid? ServiceId { get; init; }
    public DateOnly Date { get; init; }
    public int SlotDurationMinutes { get; init; } = 60;
    public int BufferMinutes { get; init; } = 0;
}

public class TimeSlot
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsAvailable { get; init; }
    public string? Reason { get; init; }
}
