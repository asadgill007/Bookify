using Bookify.Application.Commands.Notifications;
using Bookify.Application.Queries.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class NotificationsController : ApiController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationsQuery
        {
            UserId = GetUserId(),
            UnreadOnly = unreadOnly,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarkNotificationReadCommand
        {
            UserId = GetUserId(),
            NotificationId = id
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var command = new MarkAllNotificationsReadCommand
        {
            UserId = GetUserId()
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteNotificationCommand
        {
            UserId = GetUserId(),
            NotificationId = id
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
