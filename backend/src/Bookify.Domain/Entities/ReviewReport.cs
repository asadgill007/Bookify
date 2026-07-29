using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public enum ReportReason
{
    Spam = 0,
    Abuse = 1,
    Fake = 2,
    Offensive = 3,
    Other = 4
}

public enum ReportStatus
{
    Pending = 0,
    Resolved = 1,
    Dismissed = 2
}

public sealed class ReviewReport : BaseEntity
{
    public Guid ReviewId { get; private set; }
    public Guid ReportedByCustomerId { get; private set; }
    public ReportReason Reason { get; private set; }
    public string? Description { get; private set; }
    public ReportStatus Status { get; private set; }
    public string? Resolution { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    public Review Review { get; private set; } = null!;
    public User ReportedBy { get; private set; } = null!;

    private ReviewReport() { }

    public ReviewReport(Guid reviewId, Guid reportedByCustomerId, ReportReason reason, string? description = null)
    {
        ReviewId = reviewId;
        ReportedByCustomerId = reportedByCustomerId;
        Reason = reason;
        Description = description?.Trim();
        Status = ReportStatus.Pending;
    }

    public void Resolve(string? resolution = null)
    {
        Status = ReportStatus.Resolved;
        Resolution = resolution?.Trim();
        ResolvedAt = DateTime.UtcNow;
        Touch();
    }

    public void Dismiss(string? resolution = null)
    {
        Status = ReportStatus.Dismissed;
        Resolution = resolution?.Trim();
        ResolvedAt = DateTime.UtcNow;
        Touch();
    }
}
