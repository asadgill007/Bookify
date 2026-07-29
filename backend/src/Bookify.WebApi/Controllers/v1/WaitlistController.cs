using Bookify.Application.Commands.Waitlist;
using Bookify.Application.Interfaces;
using Bookify.Application.Queries.Waitlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class WaitlistController : ApiController
{
    private readonly IMediator _mediator;

    public WaitlistController(IMediator mediator) => _mediator = mediator;

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinWaitlistRequest request, CancellationToken cancellationToken)
    {
        var command = new JoinWaitlistCommand
        {
            CustomerId = GetUserId(),
            BusinessId = request.BusinessId,
            ProviderId = request.ProviderId,
            ServiceId = request.ServiceId,
            AppointmentDate = request.AppointmentDate,
            PreferredStartTime = request.PreferredStartTime,
            PreferredEndTime = request.PreferredEndTime,
            Notes = request.Notes
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? ApiCreated(result.Data!, "Joined waitlist.") : HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken cancellationToken)
    {
        var command = new LeaveWaitlistCommand { EntryId = id, UserId = GetUserId() };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/promote")]
    public async Task<IActionResult> Promote(Guid id, CancellationToken cancellationToken)
    {
        var command = new PromoteWaitlistCommand { EntryId = id, UserId = GetUserId() };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelWaitlistCommand { EntryId = id, UserId = GetUserId() };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/priority")]
    public async Task<IActionResult> UpdatePriority(Guid id, [FromBody] UpdatePriorityRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateWaitlistPriorityCommand { EntryId = id, UserId = GetUserId(), NewPriority = request.Priority };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetBusinessWaitlist(Guid businessId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetBusinessWaitlistQuery { BusinessId = businessId, UserId = GetUserId(), Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("provider/{providerId}")]
    public async Task<IActionResult> GetProviderWaitlist(Guid providerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderWaitlistQuery { ProviderId = providerId, UserId = GetUserId(), Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyWaitlist([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerWaitlistQuery { CustomerId = GetUserId(), Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("business/{businessId}/statistics")]
    public async Task<IActionResult> GetStatistics(Guid businessId, CancellationToken cancellationToken)
    {
        var query = new GetWaitlistStatisticsQuery { BusinessId = businessId, UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}

public class JoinWaitlistRequest
{
    public Guid BusinessId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ServiceId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly? PreferredStartTime { get; set; }
    public TimeOnly? PreferredEndTime { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePriorityRequest
{
    public int Priority { get; set; }
}
