using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Application.Queries.Appointments;
using Bookify.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class DashboardController : ApiController
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get customer dashboard summary with upcoming appointments and statistics.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var query = new GetCustomerDashboardQuery
        {
            UserId = GetUserId()
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get upcoming appointments (confirmed and pending).
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(CancellationToken cancellationToken)
    {
        var query = new GetAppointmentsQuery
        {
            UserId = GetUserId(),
            Role = "customer",
            From = DateTime.UtcNow,
            Page = 1,
            PageSize = 50
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get past (completed) appointments history.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAppointmentsQuery
        {
            UserId = GetUserId(),
            Role = "customer",
            Status = "completed",
            To = DateTime.UtcNow,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get business owner dashboard with revenue, booking stats, and analytics.
    /// </summary>
    [HttpGet("business/{businessId}/summary")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> GetBusinessSummary(Guid businessId, CancellationToken cancellationToken)
    {
        var query = new GetBusinessDashboardQuery
        {
            BusinessId = businessId,
            UserId = GetUserId()
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
