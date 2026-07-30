using Bookify.Application.Commands.Reviews;
using Bookify.Application.DTOs.Reviews;
using Bookify.Application.Queries.Reviews;
using Bookify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class ReviewsController : ApiController
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ─── Create Review ───────────────────────────────────
    [HttpPost("appointments/{appointmentId}")]
    [Authorize]
    public async Task<IActionResult> Create(Guid appointmentId, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand
        {
            CustomerId = GetUserId(),
            AppointmentId = appointmentId,
            Rating = request.Rating,
            Comment = request.Comment
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // ─── Get Business Reviews ────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetByBusiness(
        [FromQuery] Guid businessId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBusinessReviewsQuery { BusinessId = businessId, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    // ─── Get Provider Reviews ────────────────────────────
    [HttpGet("provider/{providerId}")]
    public async Task<IActionResult> GetByProvider(Guid providerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetProviderReviewsQuery { ProviderId = providerId, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    // ─── Get Review Statistics ───────────────────────────
    [HttpGet("statistics/{businessId}")]
    public async Task<IActionResult> GetStatistics(Guid businessId, CancellationToken cancellationToken)
    {
        var query = new GetReviewStatisticsQuery { BusinessId = businessId };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    // ─── Get Top Rated Providers ─────────────────────────
    [HttpGet("top-rated")]
    public async Task<IActionResult> GetTopRated([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var query = new GetTopRatedProvidersQuery { Count = count };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    // ─── Update Review ───────────────────────────────────
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand
        {
            ReviewId = id,
            UserId = GetUserId(),
            Rating = request.Rating,
            Comment = request.Comment
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // ─── Delete Review ───────────────────────────────────
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteReviewCommand { ReviewId = id, UserId = GetUserId() };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // ─── Reply to Review ─────────────────────────────────
    [HttpPost("{id}/reply")]
    [Authorize]
    public async Task<IActionResult> Reply(Guid id, [FromBody] ReplyRequest request, CancellationToken cancellationToken)
    {
        var command = new ReplyToReviewCommand
        {
            ReviewId = id,
            ProviderId = request.ProviderId,
            UserId = GetUserId(),
            Reply = request.Reply
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // ─── Edit Reply ──────────────────────────────────────
    [HttpPut("{id}/reply")]
    [Authorize]
    public async Task<IActionResult> EditReply(Guid id, [FromBody] EditReplyRequest request, CancellationToken cancellationToken)
    {
        var command = new EditReplyCommand { ReviewId = id, UserId = GetUserId(), Reply = request.Reply };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // ─── Delete Reply ────────────────────────────────────
    [HttpDelete("{id}/reply")]
    [Authorize]
    public async Task<IActionResult> DeleteReply(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteReplyCommand { ReviewId = id, UserId = GetUserId() };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // ─── Vote Helpful ────────────────────────────────────
    [HttpPost("{id}/vote")]
    [Authorize]
    public async Task<IActionResult> Vote(Guid id, [FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        var command = new VoteHelpfulCommand
        {
            ReviewId = id,
            CustomerId = GetUserId(),
            IsHelpful = request.IsHelpful
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // ─── Report Review ───────────────────────────────────
    [HttpPost("{id}/report")]
    [Authorize]
    public async Task<IActionResult> Report(Guid id, [FromBody] ReportReviewRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ReportReason>(request.Reason, true, out var reason))
            return ApiBadRequest("Invalid report reason. Valid reasons: Spam, Abuse, Fake, Offensive, Other.");

        var command = new ReportReviewCommand
        {
            ReviewId = id,
            ReportedByCustomerId = GetUserId(),
            Reason = reason,
            Description = request.Description
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

// ─── Request DTOs ─────────────────────────────────────────
public class ReplyRequest
{
    public Guid ProviderId { get; set; }
    public string Reply { get; set; } = string.Empty;
}

public class EditReplyRequest
{
    public string Reply { get; set; } = string.Empty;
}

public class VoteRequest
{
    public bool IsHelpful { get; set; }
}

public class ReportReviewRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
}
