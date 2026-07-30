using Bookify.Domain.Common;
using Bookify.Domain.DomainEvents;
using Bookify.Domain.Enums;

namespace Bookify.Domain.Entities;

public sealed class Appointment : BaseEntity
{
    public string BookingReference { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid BusinessId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? CustomerNotes { get; private set; }
    public bool IsCustomerNotified { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? CancellationReason { get; private set; }
    public Guid? RescheduledFromId { get; private set; }

    public User Customer { get; private set; } = null!;
    public Provider Provider { get; private set; } = null!;
    public Service Service { get; private set; } = null!;
    public Business Business { get; private set; } = null!;
    public Payment? Payment { get; private set; }
    public Review? Review { get; private set; }
    public ICollection<AppointmentLog> Logs { get; private set; } = new List<AppointmentLog>();
    public Appointment? RescheduledFrom { get; private set; }

    private Appointment() { }

    public Appointment(
        string bookingReference,
        Guid customerId,
        Guid providerId,
        Guid serviceId,
        Guid businessId,
        DateTime startTime,
        DateTime endTime,
        decimal totalAmount,
        string currency = "USD")
    {
        BookingReference = bookingReference;
        CustomerId = customerId;
        ProviderId = providerId;
        ServiceId = serviceId;
        BusinessId = businessId;
        SetTimeRange(startTime, endTime);
        TotalAmount = Math.Round(totalAmount, 2);
        Currency = currency;
        Status = AppointmentStatus.Pending;

        AddLog(null, AppointmentStatus.Pending, "Appointment created");
        AddDomainEvent(new AppointmentCreatedEvent(
            Id, CustomerId, BusinessId, ProviderId, ServiceId,
            StartTime, EndTime, TotalAmount, DateTime.UtcNow));
    }

    public void SetTimeRange(DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time.");

        if (startTime.Kind != DateTimeKind.Utc)
            startTime = startTime.ToUniversalTime();
        if (endTime.Kind != DateTimeKind.Utc)
            endTime = endTime.ToUniversalTime();

        StartTime = startTime;
        EndTime = endTime;
        Touch();
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm appointment in {Status} status.");

        var previousStatus = Status;
        Status = AppointmentStatus.Confirmed;
        AddLog(previousStatus, AppointmentStatus.Confirmed, "Appointment confirmed");
        AddDomainEvent(new AppointmentConfirmedEvent(Id, CustomerId, BusinessId, DateTime.UtcNow));
        Touch();
    }

    public void Start()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException($"Cannot start appointment in {Status} status.");

        var previousStatus = Status;
        Status = AppointmentStatus.InProgress;
        AddLog(previousStatus, AppointmentStatus.InProgress, "Appointment in progress");
        Touch();
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete appointment in {Status} status.");

        var previousStatus = Status;
        Status = AppointmentStatus.Completed;
        AddLog(previousStatus, AppointmentStatus.Completed, "Appointment completed");
        AddDomainEvent(new AppointmentCompletedEvent(Id, CustomerId, ProviderId, BusinessId, DateTime.UtcNow));
        Touch();
    }

    public void Cancel(string? reason = null)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel appointment in {Status} status.");

        var previousStatus = Status;
        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason?.Trim();
        AddLog(previousStatus, AppointmentStatus.Cancelled, reason ?? "Appointment cancelled");
        AddDomainEvent(new AppointmentCancelledEvent(Id, CustomerId, BusinessId, reason, DateTime.UtcNow));
        Touch();
    }

    public void MarkNoShow()
    {
        if (Status != AppointmentStatus.Confirmed && Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException($"Cannot mark no-show in {Status} status.");

        var previousStatus = Status;
        Status = AppointmentStatus.NoShow;
        AddLog(previousStatus, AppointmentStatus.NoShow, "Customer did not show");
        Touch();
    }

    public Appointment Reschedule(DateTime newStartTime, DateTime newEndTime)
    {
        var previousStatus = Status;
        Status = AppointmentStatus.Rescheduled;
        AddLog(previousStatus, AppointmentStatus.Rescheduled, "Appointment rescheduled");

        var newAppointment = new Appointment(
            BookingReference + "-R",
            CustomerId,
            ProviderId,
            ServiceId,
            BusinessId,
            newStartTime,
            newEndTime,
            TotalAmount,
            Currency)
        {
            RescheduledFromId = Id
        };

        Touch();
        return newAppointment;
    }

    public void MarkNotified()
    {
        IsCustomerNotified = true;
        Touch();
    }

    public void SetNotes(string? notes)
    {
        CustomerNotes = notes?.Trim();
        Touch();
    }

    private void AddLog(AppointmentStatus? fromStatus, AppointmentStatus toStatus, string reason)
    {
        Logs.Add(new AppointmentLog(Id, fromStatus, toStatus, reason));
    }
}

public sealed class AppointmentLog : BaseEntity
{
    public Guid AppointmentId { get; private set; }
    public AppointmentStatus? FromStatus { get; private set; }
    public AppointmentStatus ToStatus { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public string? Reason { get; private set; }

    public Appointment Appointment { get; private set; } = null!;

    private AppointmentLog() { }

    public AppointmentLog(
        Guid appointmentId,
        AppointmentStatus? fromStatus,
        AppointmentStatus toStatus,
        string? reason = null,
        Guid? changedByUserId = null)
    {
        AppointmentId = appointmentId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Reason = reason?.Trim();
        ChangedByUserId = changedByUserId;
    }
}
