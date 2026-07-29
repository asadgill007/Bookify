using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public sealed class Provider : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid BusinessId { get; private set; }
    public string? Title { get; private set; }
    public string? Bio { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    public User User { get; private set; } = null!;
    public Business Business { get; private set; } = null!;
    public ICollection<ProviderAvailability> Availabilities { get; private set; } = new List<ProviderAvailability>();
    public ICollection<Appointment> Appointments { get; private set; } = new List<Appointment>();
    public ICollection<ProviderService> ProviderServices { get; private set; } = new List<ProviderService>();
    public ICollection<ProviderAvailabilityOverride> AvailabilityOverrides { get; private set; } = new List<ProviderAvailabilityOverride>();

    private Provider() { }

    public Provider(Guid userId, Guid businessId, string? title = null)
    {
        UserId = userId;
        BusinessId = businessId;
        Title = title?.Trim();
        IsActive = true;
    }

    public void UpdateDetails(string? title, string? bio, int displayOrder)
    {
        Title = title?.Trim();
        Bio = bio?.Trim();
        DisplayOrder = displayOrder;
        Touch();
    }

    public void ToggleActive(bool active)
    {
        IsActive = active;
        Touch();
    }
}

public sealed class ProviderService : BaseEntity
{
    public Guid ProviderId { get; private set; }
    public Guid ServiceId { get; private set; }

    public Provider Provider { get; private set; } = null!;
    public Service Service { get; private set; } = null!;

    private ProviderService() { }

    public ProviderService(Guid providerId, Guid serviceId)
    {
        ProviderId = providerId;
        ServiceId = serviceId;
    }
}

public sealed class ProviderAvailability : BaseEntity
{
    public Guid ProviderId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsAvailable { get; private set; }
    public int SlotDurationMinutes { get; private set; }

    public Provider Provider { get; private set; } = null!;

    private ProviderAvailability() { }

    public ProviderAvailability(
        Guid providerId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int slotDurationMinutes = 60)
    {
        ProviderId = providerId;
        DayOfWeek = dayOfWeek;
        SetTimeRange(startTime, endTime);
        IsAvailable = true;
        SlotDurationMinutes = slotDurationMinutes;
    }

    public void SetTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time.");

        StartTime = startTime;
        EndTime = endTime;
        Touch();
    }

    public void SetSlotDuration(int minutes)
    {
        if (minutes < 15 || minutes > 480)
            throw new ArgumentException("Slot duration must be between 15 and 480 minutes.");

        SlotDurationMinutes = minutes;
        Touch();
    }

    public void ToggleAvailable(bool available)
    {
        IsAvailable = available;
        Touch();
    }
}

public sealed class ProviderAvailabilityOverride : BaseEntity
{
    public Guid ProviderId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public bool IsAvailable { get; private set; }
    public string? Reason { get; private set; }

    public Provider Provider { get; private set; } = null!;

    private ProviderAvailabilityOverride() { }

    public ProviderAvailabilityOverride(
        Guid providerId,
        DateOnly date,
        bool isAvailable,
        string? reason = null)
    {
        ProviderId = providerId;
        Date = date;
        IsAvailable = isAvailable;
        Reason = reason?.Trim();
    }

    public ProviderAvailabilityOverride(
        Guid providerId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        string? reason = null)
    {
        ProviderId = providerId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = true;
        Reason = reason?.Trim();
    }

    public void Update(bool isAvailable, TimeOnly? startTime, TimeOnly? endTime, string? reason)
    {
        IsAvailable = isAvailable;
        StartTime = startTime;
        EndTime = endTime;
        Reason = reason?.Trim();
        Touch();
    }
}
