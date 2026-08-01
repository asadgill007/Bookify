using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Chat;

/// <summary>
/// Get the current user's persisted chat history (newest first).
/// </summary>
public sealed record GetChatHistoryQuery : IRequest<Result<IReadOnlyList<ChatHistoryItem>>>
{
    public Guid UserId { get; init; }
    public int Limit { get; init; } = 100;
}

public sealed class ChatHistoryItem
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, Result<IReadOnlyList<ChatHistoryItem>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetChatHistoryQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<IReadOnlyList<ChatHistoryItem>>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var messages = await _unitOfWork.ChatMessages.GetByUserAsync(request.UserId, request.Limit, cancellationToken);

        var items = messages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ChatHistoryItem
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return Result<IReadOnlyList<ChatHistoryItem>>.Success(items);
    }
}
