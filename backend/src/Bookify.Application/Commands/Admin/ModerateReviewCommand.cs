using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Admin;

public sealed record ModerateReviewCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid ReviewId { get; init; }
    public bool IsPublished { get; init; }
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
        if (review == null)
            return Result.Failure("Review not found.", "NOT_FOUND");

        review.Moderate(request.IsPublished);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var action = request.IsPublished ? "published" : "unpublished";
        _logger.LogInformation(
            "Admin {AdminId} {Action} review {ReviewId}",
            request.AdminUserId, action, request.ReviewId);

        return Result.Success();
    }
}
