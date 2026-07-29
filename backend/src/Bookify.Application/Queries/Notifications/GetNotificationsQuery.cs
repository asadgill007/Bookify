using Bookify.Application.Common;
using Bookify.Application.DTOs.Notifications;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Notifications;

public sealed record GetNotificationsQuery : IRequest<Result<IReadOnlyList<NotificationDto>>>
{
    public Guid UserId { get; init; }
    public bool? UnreadOnly { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(
            request.UserId, request.UnreadOnly, request.Page, request.PageSize, cancellationToken);

        var items = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Title = n.Title,
            Body = n.Body,
            Data = n.Data,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return Result<IReadOnlyList<NotificationDto>>.Success(items);
    }
}
