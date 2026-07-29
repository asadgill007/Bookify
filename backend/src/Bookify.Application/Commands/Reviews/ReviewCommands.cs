using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Reviews;

// ─── Create Review ───────────────────────────────────────
public sealed record CreateReviewCommand : IRequest<Result<Guid>>
{
    public Guid CustomerId { get; init; }
    public Guid AppointmentId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
}

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.Comment).MaximumLength(2000).When(x => x.Comment != null);
    }
}

public sealed class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result<Guid>.Failure("Appointment not found.", "NOT_FOUND");
        if (appointment.Status != Domain.Enums.AppointmentStatus.Completed)
            return Result<Guid>.Failure("Can only review completed appointments.", "INVALID_STATUS");
        if (appointment.CustomerId != request.CustomerId)
            return Result<Guid>.Failure("You can only review your own appointments.", "FORBIDDEN");

        var hasReview = await _unitOfWork.Reviews.HasReviewForAppointmentAsync(request.AppointmentId, cancellationToken);
        if (hasReview)
            return Result<Guid>.Failure("You have already reviewed this appointment.", "DUPLICATE");

        var review = new Review(request.AppointmentId, appointment.BusinessId, request.CustomerId, request.Rating, request.Comment);
        await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review {ReviewId} created for appointment {AppointmentId}", review.Id, request.AppointmentId);
        return Result<Guid>.Success(review.Id);
    }
}

// ─── Update Review ───────────────────────────────────────
public sealed record UpdateReviewCommand : IRequest<Result>
{
    public Guid ReviewId { get; init; }
    public Guid UserId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
}

public sealed class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(2000).When(x => x.Comment != null);
    }
}

public sealed class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateReviewCommandHandler> _logger;

    public UpdateReviewCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null) return Result.Failure("Review not found.", "NOT_FOUND");
        if (review.CustomerId != request.UserId) return Result.Failure("Access denied.", "FORBIDDEN");

        review.Update(request.Rating, request.Comment);

        var (avgRating, totalReviews) = await _unitOfWork.Reviews.GetBusinessRatingAsync(review.BusinessId, cancellationToken);
        var business = await _unitOfWork.Businesses.GetByIdAsync(review.BusinessId, cancellationToken);
        if (business != null) business.UpdateRating(avgRating, totalReviews);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Review {ReviewId} updated by user {UserId}", request.ReviewId, request.UserId);
        return Result.Success();
    }
}

// ─── Delete Review ───────────────────────────────────────
public sealed record DeleteReviewCommand : IRequest<Result>
{
    public Guid ReviewId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReviewCommandHandler> _logger;

    public DeleteReviewCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null) return Result.Failure("Review not found.", "NOT_FOUND");
        if (review.CustomerId != request.UserId) return Result.Failure("Access denied.", "FORBIDDEN");

        var businessId = review.BusinessId;

        review.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recalculate business rating after deletion
        var (avgRating, totalReviews) = await _unitOfWork.Reviews.GetBusinessRatingAsync(businessId, cancellationToken);
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId, cancellationToken);
        if (business != null)
        {
            business.UpdateRating(avgRating, totalReviews);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Review {ReviewId} deleted by user {UserId}", request.ReviewId, request.UserId);
        return Result.Success();
    }
}

// ─── Reply to Review ─────────────────────────────────────
public sealed record ReplyToReviewCommand : IRequest<Result>
{
    public Guid ReviewId { get; init; }
    public Guid ProviderId { get; init; }
    public Guid UserId { get; init; }
    public string Reply { get; init; } = string.Empty;
}

public sealed class ReplyToReviewCommandValidator : AbstractValidator<ReplyToReviewCommand>
{
    public ReplyToReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Reply).NotEmpty().MaximumLength(2000);
    }
}

public sealed class ReplyToReviewCommandHandler : IRequestHandler<ReplyToReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReplyToReviewCommandHandler> _logger;

    public ReplyToReviewCommandHandler(IUnitOfWork unitOfWork, ILogger<ReplyToReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ReplyToReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null) return Result.Failure("Review not found.", "NOT_FOUND");

        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null) return Result.Failure("Provider not found.", "NOT_FOUND");
        if (provider.UserId != request.UserId)
            return Result.Failure("You are not authorized to reply as this provider.", "FORBIDDEN");
        if (provider.BusinessId != review.BusinessId)
            return Result.Failure("Provider does not belong to the review's business.", "PROVIDER_MISMATCH");

        review.Reply(request.ProviderId, request.Reply);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Provider {ProviderId} replied to review {ReviewId}", request.ProviderId, request.ReviewId);
        return Result.Success();
    }
}

// ─── Edit Reply ──────────────────────────────────────────
public sealed record EditReplyCommand : IRequest<Result>
{
    public Guid ReviewId { get; init; }
    public Guid UserId { get; init; }
    public string Reply { get; init; } = string.Empty;
}

public sealed class EditReplyCommandValidator : AbstractValidator<EditReplyCommand>
{
    public EditReplyCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Reply).NotEmpty().MaximumLength(2000);
    }
}

public sealed class EditReplyCommandHandler : IRequestHandler<EditReplyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EditReplyCommandHandler> _logger;

    public EditReplyCommandHandler(IUnitOfWork unitOfWork, ILogger<EditReplyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(EditReplyCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null) return Result.Failure("Review not found.", "NOT_FOUND");
        if (review.ProviderReply == null) return Result.Failure("No reply exists to edit.", "NO_REPLY");

        var provider = await _unitOfWork.Providers.GetByIdAsync(review.ProviderId!.Value, cancellationToken);
        if (provider == null) return Result.Failure("Provider not found.", "NOT_FOUND");
        if (provider.UserId != request.UserId) return Result.Failure("Access denied.", "FORBIDDEN");

        review.EditReply(request.Reply);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reply edited on review {ReviewId}", request.ReviewId);
        return Result.Success();
    }
}

// ─── Delete Reply ────────────────────────────────────────
public sealed record DeleteReplyCommand : IRequest<Result>
{
    public Guid ReviewId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class DeleteReplyCommandValidator : AbstractValidator<DeleteReplyCommand>
{
    public DeleteReplyCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class DeleteReplyCommandHandler : IRequestHandler<DeleteReplyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReplyCommandHandler> _logger;

    public DeleteReplyCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteReplyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteReplyCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null) return Result.Failure("Review not found.", "NOT_FOUND");
        if (review.ProviderReply == null) return Result.Failure("No reply exists to delete.", "NO_REPLY");

        var provider = await _unitOfWork.Providers.GetByIdAsync(review.ProviderId!.Value, cancellationToken);
        if (provider == null) return Result.Failure("Provider not found.", "NOT_FOUND");
        if (provider.UserId != request.UserId) return Result.Failure("Access denied.", "FORBIDDEN");

        review.DeleteReply();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reply deleted from review {ReviewId}", request.ReviewId);
        return Result.Success();
    }
}

// ─── Moderate Review ─────────────────────────────────────
public sealed record ModerateReviewCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid ReviewId { get; init; }
    public string Action { get; init; } = string.Empty; // approve, reject, hide, restore
    public string? Reason { get; init; }
}

public sealed class ModerateReviewCommandValidator : AbstractValidator<ModerateReviewCommand>
{
    public ModerateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty().Must(a => new[] { "approve", "reject", "hide", "restore" }.Contains(a))
            .WithMessage("Action must be one of: approve, reject, hide, restore.");
    }
}

public sealed class ModerateReviewCommandHandler : IRequestHandler<ModerateReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ModerateReviewCommandHandler> _logger;

    public ModerateReviewCommandHandler(IUnitOfWork unitOfWork, ILogger<ModerateReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ModerateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null) return Result.Failure("Review not found.", "NOT_FOUND");

        switch (request.Action.ToLowerInvariant())
        {
            case "approve":
                review.Moderate(true, request.Reason);
                break;
            case "reject":
                review.Moderate(false, request.Reason);
                review.SoftDelete();
                break;
            case "hide":
                review.Hide(request.Reason);
                break;
            case "restore":
                review.Restore();
                break;
            default:
                return Result.Failure("Invalid moderation action.", "INVALID_ACTION");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} performed '{Action}' on review {ReviewId}",
            request.AdminUserId, request.Action, request.ReviewId);

        return Result.Success();
    }
}

// ─── Vote Helpful ────────────────────────────────────────
public sealed record VoteHelpfulCommand : IRequest<Result>
{
    public Guid ReviewId { get; init; }
    public Guid CustomerId { get; init; }
    public bool IsHelpful { get; init; }
}

public sealed class VoteHelpfulCommandValidator : AbstractValidator<VoteHelpfulCommand>
{
    public VoteHelpfulCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}

public sealed class VoteHelpfulCommandHandler : IRequestHandler<VoteHelpfulCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VoteHelpfulCommandHandler> _logger;

    public VoteHelpfulCommandHandler(IUnitOfWork unitOfWork, ILogger<VoteHelpfulCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(VoteHelpfulCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null) return Result.Failure("Review not found.", "NOT_FOUND");

        var hasVoted = await _unitOfWork.Reviews.HasCustomerVotedAsync(request.ReviewId, request.CustomerId, cancellationToken);
        if (hasVoted)
            return Result.Failure("You have already voted on this review.", "DUPLICATE_VOTE");

        var vote = new ReviewVote(request.ReviewId, request.CustomerId, request.IsHelpful);
        await _unitOfWork.Reviews.AddVoteAsync(vote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} voted {IsHelpful} on review {ReviewId}",
            request.CustomerId, request.IsHelpful ? "helpful" : "not helpful", request.ReviewId);

        return Result.Success();
    }
}

// ─── Report Review ───────────────────────────────────────
public sealed record ReportReviewCommand : IRequest<Result>
{
    public Guid ReviewId { get; init; }
    public Guid ReportedByCustomerId { get; init; }
    public ReportReason Reason { get; init; }
    public string? Description { get; init; }
}

public sealed class ReportReviewCommandValidator : AbstractValidator<ReportReviewCommand>
{
    public ReportReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.ReportedByCustomerId).NotEmpty();
        RuleFor(x => x.Reason).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
    }
}

public sealed class ReportReviewCommandHandler : IRequestHandler<ReportReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportReviewCommandHandler> _logger;

    public ReportReviewCommandHandler(IUnitOfWork unitOfWork, ILogger<ReportReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ReportReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null) return Result.Failure("Review not found.", "NOT_FOUND");

        var report = new ReviewReport(request.ReviewId, request.ReportedByCustomerId, request.Reason, request.Description);
        await _unitOfWork.Reviews.AddReportAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} reported review {ReviewId} as {Reason}",
            request.ReportedByCustomerId, request.ReviewId, request.Reason);

        return Result.Success();
    }
}
