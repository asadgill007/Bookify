using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public enum WaitlistStatus
{
    Waiting = 0,
    Notified = 1,
    Promoted = 2,
    Expired = 3,
    Cancelled = 4
}

public sealed class WaitlistEntry : BaseEntity
{
    public Guid BusinessId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateOnly AppointmentDate { get; private set; }
    public TimeOnly? PreferredStartTime { get; private set; }
    public TimeOnly? PreferredEndTime { get; private set; }
    public string? Notes { get; private set; }
    public WaitlistStatus Status { get; private set; }
    public int Priority { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public DateTime? PromotedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public Business Business { get; private set; } = null!;
    public Provider Provider { get; private set; } = null!;
    public Service Service { get; private set; } = null!;
    public User Customer { get; private set; } = null!;

    private WaitlistEntry() { }

    public WaitlistEntry(
        Guid businessId,
        Guid providerId,
        Guid serviceId,
        Guid customerId,
        DateOnly appointmentDate,
        TimeOnly? preferredStartTime = null,
        TimeOnly? preferredEndTime = null,
        string? notes = null,
        int priority = 0)
    {
        BusinessId = businessId;
        ProviderId = providerId;
        ServiceId = serviceId;
        CustomerId = customerId;
        AppointmentDate = appointmentDate;
        PreferredStartTime = preferredStartTime;
        PreferredEndTime = preferredEndTime;
        Notes = notes?.Trim();
        Status = WaitlistStatus.Waiting;
        Priority = priority;
        ExpiresAt = DateTime.UtcNow.AddDays(7);

        if (appointmentDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Appointment date cannot be in the past.", nameof(appointmentDate));
    }

    public void Promote()
    {
        if (Status != WaitlistStatus.Waiting && Status != WaitlistStatus.Notified)
            throw new InvalidOperationException($"Cannot promote entry in {Status} status.");

        Status = WaitlistStatus.Promoted;
        PromotedAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkNotified()
    {
        Status = WaitlistStatus.Notified;
        NotifiedAt = DateTime.UtcNow;
        Touch();
    }

    public void Cancel()
    {
        Status = WaitlistStatus.Cancelled;
        Touch();
    }

    public void Expire()
    {
        Status = WaitlistStatus.Expired;
        Touch();
    }

    public void UpdatePriority(int newPriority)
    {
        Priority = newPriority;
        Touch();
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
