using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

/// <summary>
/// A single persisted chat message in the customer-support AI chat.
/// Role is "user" or "assistant".
/// </summary>
public sealed class ChatMessage : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = null!;
    public string Content { get; private set; } = null!;

    public User User { get; private set; } = null!;

    private ChatMessage() { }

    public ChatMessage(Guid userId, string role, string content)
    {
        UserId = userId;
        SetRole(role);
        Content = content.Trim();
    }

    public void SetRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty.", nameof(role));
        if (role != "user" && role != "assistant")
            throw new ArgumentException("Role must be 'user' or 'assistant'.", nameof(role));
        Role = role.ToLowerInvariant();
    }
}
