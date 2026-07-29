using Bookify.Domain.Common;
using Bookify.Domain.DomainEvents;

namespace Bookify.Domain.Entities;

public sealed class Review : BaseEntity
{
    public Guid AppointmentId { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }
    public bool IsPublished { get; private set; }

    // Provider Reply
    public Guid? ProviderId { get; private set; }
    public string? ProviderReply { get; private set; }
    public DateTime? RepliedAt { get; private set; }
    public DateTime? ReplyUpdatedAt { get; private set; }

    // Moderation
    public bool IsHidden { get; private set; }
    public DateTime? HiddenAt { get; private set; }
    public string? HideReason { get; private set; }
    public string? ModerationReason { get; private set; }

    public Appointment Appointment { get; private set; } = null!;
    public Business Business { get; private set; } = null!;
    public User Customer { get; private set; } = null!;
    public Provider? Provider { get; private set; }

    private Review() { }

    public Review(Guid appointmentId, Guid businessId, Guid customerId, int rating, string? comment = null)
    {
        AppointmentId = appointmentId;
        BusinessId = businessId;
        CustomerId = customerId;
        SetRating(rating);
        Comment = comment?.Trim();
        IsVerifiedPurchase = true;
        IsPublished = true;

        AddDomainEvent(new ReviewSubmittedEvent(Id, AppointmentId, BusinessId, rating, DateTime.UtcNow));
    }

    public void SetRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        Rating = rating;
        Touch();
    }

    public void Update(int rating, string? comment)
    {
        SetRating(rating);
        Comment = comment?.Trim();
        IsPublished = true;
        Touch();
    }

    public void Moderate(bool isPublished, string? reason = null)
    {
        IsPublished = isPublished;
        ModerationReason = reason;
        Touch();
    }

    public void Reply(Guid providerId, string reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
            throw new ArgumentException("Reply cannot be empty.", nameof(reply));
        if (ProviderReply != null)
            throw new InvalidOperationException("A reply already exists for this review.");

        ProviderId = providerId;
        ProviderReply = reply.Trim();
        RepliedAt = DateTime.UtcNow;
        Touch();
    }

    public void EditReply(string reply)
    {
        if (ProviderReply == null)
            throw new InvalidOperationException("No reply exists to edit.");
        if (string.IsNullOrWhiteSpace(reply))
            throw new ArgumentException("Reply cannot be empty.", nameof(reply));

        ProviderReply = reply.Trim();
        ReplyUpdatedAt = DateTime.UtcNow;
        Touch();
    }

    public void DeleteReply()
    {
        if (ProviderReply == null)
            throw new InvalidOperationException("No reply exists to delete.");

        ProviderId = null;
        ProviderReply = null;
        RepliedAt = null;
        ReplyUpdatedAt = null;
        Touch();
    }

    public void Hide(string? reason = null)
    {
        IsHidden = true;
        HiddenAt = DateTime.UtcNow;
        HideReason = reason;
        IsPublished = false;
        Touch();
    }

    public void Restore()
    {
        IsHidden = false;
        HiddenAt = null;
        HideReason = null;
        IsPublished = true;
        Touch();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        Touch();
    }
}
