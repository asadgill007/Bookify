using Bookify.Application.Commands.Admin;
using Bookify.Application.Queries.Admin;
using Bookify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class AdminController : ApiController
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Dashboard

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var query = new GetAdminDashboardQuery { AdminUserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    #endregion

    #region Users

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role,
        [FromQuery] bool? suspended,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminUsersQuery
        {
            Role = role,
            Suspended = suspended,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("users/{userId}/role")]
    public async Task<IActionResult> ChangeUserRole(Guid userId, [FromBody] ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeUserRoleCommand
        {
            AdminUserId = GetUserId(),
            TargetUserId = userId,
            NewRole = request.Role
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("users/{userId}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid userId, [FromBody] SuspendRequest request, CancellationToken cancellationToken)
    {
        var command = new SuspendUserCommand
        {
            AdminUserId = GetUserId(),
            TargetUserId = userId,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("users/{userId}/unsuspend")]
    public async Task<IActionResult> UnsuspendUser(Guid userId, CancellationToken cancellationToken)
    {
        var command = new UnsuspendUserCommand
        {
            AdminUserId = GetUserId(),
            TargetUserId = userId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand
        {
            AdminUserId = GetUserId(),
            TargetUserId = userId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    #endregion

    #region Businesses

    [HttpGet("businesses")]
    public async Task<IActionResult> GetBusinesses(
        [FromQuery] bool? verified,
        [FromQuery] bool? active,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminBusinessesQuery
        {
            Verified = verified,
            Active = active,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("businesses/{businessId}/verify")]
    public async Task<IActionResult> VerifyBusiness(Guid businessId, CancellationToken cancellationToken)
    {
        var command = new VerifyBusinessCommand
        {
            AdminUserId = GetUserId(),
            BusinessId = businessId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("businesses/{businessId}/status")]
    public async Task<IActionResult> ToggleBusinessStatus(Guid businessId, [FromBody] ToggleStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ToggleBusinessActiveCommand
        {
            AdminUserId = GetUserId(),
            BusinessId = businessId,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    #endregion

    #region Reviews

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews(
        [FromQuery] bool? published,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminReviewsQuery
        {
            Published = published,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("reviews/{reviewId}/moderate")]
    public async Task<IActionResult> ModerateReview(Guid reviewId, [FromBody] ModerateRequest request, CancellationToken cancellationToken)
    {
        var command = new ModerateReviewCommand
        {
            AdminUserId = GetUserId(),
            ReviewId = reviewId,
            IsPublished = request.IsPublished
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    #endregion
}

public class ChangeRoleRequest
{
    public UserRole Role { get; set; }
}

public class SuspendRequest
{
    public string? Reason { get; set; }
}

public class ToggleStatusRequest
{
    public bool IsActive { get; set; }
}

public class ModerateRequest
{
    public bool IsPublished { get; set; }
}
