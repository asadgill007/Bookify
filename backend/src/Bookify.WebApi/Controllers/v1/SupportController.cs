using Bookify.Application.Commands.Support;
using Bookify.Application.Queries.Support;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class SupportController : ApiController
{
    private readonly IMediator _mediator;

    public SupportController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a support ticket (Contact Support or Report a Problem).</summary>
    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSupportTicketCommand
        {
            UserId = GetUserId(),
            Category = request.Category,
            Subject = request.Subject,
            Message = request.Message,
            AppointmentId = request.AppointmentId,
            ContactEmail = request.ContactEmail
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? ApiCreated(result.Data, "Support ticket submitted. We'll get back to you soon.")
            : HandleResult(result);
    }

    /// <summary>List the current user's support tickets.</summary>
    [HttpGet("tickets")]
    public async Task<IActionResult> GetMyTickets(CancellationToken cancellationToken)
    {
        var query = new GetMySupportTicketsQuery { UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}

public class CreateSupportTicketRequest
{
    public string Category { get; set; } = "General";
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string? ContactEmail { get; set; }
}
