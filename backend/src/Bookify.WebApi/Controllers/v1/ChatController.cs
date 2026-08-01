using Bookify.Application.Commands.Chat;
using Bookify.Application.Queries.Chat;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class ChatController : ApiController
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator) => _mediator = mediator;

    /// <summary>Send a message to the AI assistant (persists history).</summary>
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var command = new SendChatMessageCommand
        {
            UserId = GetUserId(),
            Message = request.Message
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? ApiOk(result.Data)
            : HandleResult(result);
    }

    /// <summary>Get the current user's persisted chat history (newest first).</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        var query = new GetChatHistoryQuery { UserId = GetUserId(), Limit = Math.Clamp(limit, 1, 200) };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}

public class SendMessageRequest
{
    public string Message { get; set; } = string.Empty;
}
