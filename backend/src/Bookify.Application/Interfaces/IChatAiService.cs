using Bookify.Application.Common;

namespace Bookify.Application.Interfaces;

/// <summary>
/// One message in a chat conversation with the AI assistant.
/// </summary>
public sealed class ChatAiTurn
{
    public string Role { get; init; } = "user"; // "user" | "assistant"
    public string Content { get; init; } = string.Empty;
}

public sealed class ChatAiRequest
{
    /// <summary>Full conversation history (oldest first) including the new user message.</summary>
    public IReadOnlyList<ChatAiTurn> Messages { get; init; } = Array.Empty<ChatAiTurn>();
    /// <summary>Requesting user id (for booking-status lookups).</summary>
    public Guid? UserId { get; init; }
}

public sealed class ChatAiReply
{
    public string Content { get; init; } = string.Empty;
    /// <summary>True when the reply was produced by a real AI provider.</summary>
    public bool UsedExternalProvider { get; init; }
}

/// <summary>
/// AI assistant used by the customer chatbot. Implementations may call a real
/// LLM provider (OpenAI, Groq, Gemini, ...) or fall back to rule-based canned
/// responses when no API key is configured so the feature is always usable.
/// </summary>
public interface IChatAiService
{
    Task<Result<ChatAiReply>> AskAsync(ChatAiRequest request, CancellationToken cancellationToken = default);
}
