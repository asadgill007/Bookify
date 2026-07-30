namespace Bookify.Application.Interfaces;

/// <summary>
/// Chat/Messaging service interface for real-time communication.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Send a message in a conversation.
    /// </summary>
    Task<ChatMessageResult> SendMessageAsync(
        Guid conversationId,
        Guid senderId,
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a message as read.
    /// </summary>
    Task<bool> MarkAsReadAsync(
        Guid messageId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a message.
    /// </summary>
    Task<bool> DeleteMessageAsync(
        Guid messageId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

public class ChatMessageResult
{
    public bool IsSuccess { get; set; }
    public Guid? MessageId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}