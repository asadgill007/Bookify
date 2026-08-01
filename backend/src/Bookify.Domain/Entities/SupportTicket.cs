using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public enum SupportTicketStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}

/// <summary>
/// A customer support ticket submitted from the app (contact support /
/// report a problem). Optionally linked to an appointment for context.
/// </summary>
public sealed class SupportTicket : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Category { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public Guid? AppointmentId { get; private set; }
    public string? ContactEmail { get; private set; }
    public SupportTicketStatus Status { get; private set; }

    public User User { get; private set; } = null!;
    public Appointment? Appointment { get; private set; }

    private SupportTicket() { }

    public SupportTicket(
        Guid userId,
        string category,
        string subject,
        string message,
        Guid? appointmentId = null,
        string? contactEmail = null)
    {
        UserId = userId;
        Category = category.Trim();
        Subject = subject.Trim();
        Message = message.Trim();
        AppointmentId = appointmentId;
        ContactEmail = contactEmail?.Trim();
        Status = SupportTicketStatus.Open;
    }

    public void Resolve()
    {
        Status = SupportTicketStatus.Resolved;
        Touch();
    }
}
