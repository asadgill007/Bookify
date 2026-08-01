using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Chat;

/// <summary>
/// Send a message to the AI assistant. Persists the user message and the
/// assistant reply so chat history survives app restarts.
/// </summary>
public sealed record SendChatMessageCommand : IRequest<Result<ChatReplyResult>>
{
    public Guid UserId { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public sealed class ChatReplyResult
{
    public Guid MessageId { get; set; }
    public string Reply { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool UsedExternalProvider { get; set; }
}

public sealed class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, Result<ChatReplyResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatAiService _chatAiService;
    private readonly ILogger<SendChatMessageCommandHandler> _logger;

    public SendChatMessageCommandHandler(
        IUnitOfWork unitOfWork,
        IChatAiService chatAiService,
        ILogger<SendChatMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _chatAiService = chatAiService;
        _logger = logger;
    }

    public async Task<Result<ChatReplyResult>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        // Load recent history (oldest first) to give the assistant context.
        var history = await _unitOfWork.ChatMessages.GetByUserAsync(request.UserId, limit: 50, cancellationToken);
        var turns = history
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatAiTurn { Role = m.Role, Content = m.Content })
            .ToList();

        turns.Add(new ChatAiTurn { Role = "user", Content = request.Message });

        var aiRequest = new ChatAiRequest
        {
            Messages = turns,
            UserId = request.UserId
        };

        var reply = await _chatAiService.AskAsync(aiRequest, cancellationToken);
        if (reply.IsFailure)
            return Result<ChatReplyResult>.Failure(reply.Error ?? "Chat assistant failed.", "CHAT_ERROR");

        // Persist both messages.
        var userMessage = new ChatMessage(request.UserId, "user", request.Message);
        var assistantMessage = new ChatMessage(request.UserId, "assistant", reply.Data!.Content);

        await _unitOfWork.ChatMessages.AddAsync(userMessage, cancellationToken);
        await _unitOfWork.ChatMessages.AddAsync(assistantMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chat: user {UserId} sent message; assistant replied ({External})",
            request.UserId, reply.Data.UsedExternalProvider);

        return Result<ChatReplyResult>.Success(new ChatReplyResult
        {
            MessageId = assistantMessage.Id,
            Reply = reply.Data.Content,
            SentAt = assistantMessage.CreatedAt,
            UsedExternalProvider = reply.Data.UsedExternalProvider
        });
    }
}
