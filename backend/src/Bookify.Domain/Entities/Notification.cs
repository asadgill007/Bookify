using Bookify.Domain.Common;
using Bookify.Domain.Enums;

namespace Bookify.Domain.Entities;

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public string? Data { get; private set; }
    public bool IsRead { get; private set; }

    public User User { get; private set; } = null!;

    private Notification() { }

    public Notification(Guid userId, NotificationType type, string title, string body, string? data = null)
    {
        UserId = userId;
        Type = type;
        SetContent(title, body);
        Data = data;
    }

    public void SetContent(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Notification title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Notification body cannot be empty.", nameof(body));

        Title = title.Trim();
        Body = body.Trim();
        Touch();
    }

    public void MarkAsRead()
    {
        IsRead = true;
        Touch();
    }
}
