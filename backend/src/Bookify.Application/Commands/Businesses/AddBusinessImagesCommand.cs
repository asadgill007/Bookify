using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Businesses;

/// <summary>
/// Add one or more gallery images (by URL) to a business. The first image
/// becomes the cover when the business has none yet. Triggers the
/// auto-verification checklist (at least one image is required to go live).
/// </summary>
public sealed record AddBusinessImagesCommand : IRequest<Result>
{
    public Guid BusinessId { get; init; }
    public Guid UserId { get; init; }
    public IReadOnlyList<string> ImageUrls { get; init; } = Array.Empty<string>();
}

public sealed class AddBusinessImagesCommandValidator : AbstractValidator<AddBusinessImagesCommand>
{
    public AddBusinessImagesCommandValidator()
    {
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ImageUrls).NotNull().Must(urls => urls.Count > 0).WithMessage("At least one image URL is required.");
        RuleForEach(x => x.ImageUrls)
            .NotEmpty().MaximumLength(1000)
            .Must(u => u.StartsWith("http://") || u.StartsWith("https://") || u.StartsWith("/"))
            .WithMessage("Image URL must be an http(s) URL or a server path.");
    }
}

public sealed class AddBusinessImagesCommandHandler : IRequestHandler<AddBusinessImagesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly IBusinessVerificationService _verificationService;
    private readonly ILogger<AddBusinessImagesCommandHandler> _logger;

    public AddBusinessImagesCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        IBusinessVerificationService verificationService,
        ILogger<AddBusinessImagesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _verificationService = verificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(AddBusinessImagesCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdWithDetailsAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageBusinessAsync(request.UserId, business.Id, cancellationToken))
            return Result.Failure("You do not have permission to update this business.", "FORBIDDEN");

        var hasExistingCover = business.Images.Any(i => i.IsCover && !i.IsDeleted)
                               || !string.IsNullOrWhiteSpace(business.CoverImageUrl);

        var displayOrder = business.Images.Count > 0
            ? business.Images.Max(i => i.DisplayOrder) + 1
            : 1;

        foreach (var url in request.ImageUrls)
        {
            var order = displayOrder++;
            var isCover = !hasExistingCover && order == 1;
            var image = new BusinessImage(
                business.Id,
                url.Trim(),
                null,
                order,
                isCover);
            hasExistingCover = hasExistingCover || isCover;

            await _unitOfWork.Businesses.AddImageAsync(image, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Image present is a checklist item — re-evaluate.
        await _verificationService.EvaluateAndAutoVerifyAsync(business.Id, cancellationToken);

        _logger.LogInformation("Added {Count} images to business {BusinessId} by {UserId}",
            request.ImageUrls.Count, business.Id, request.UserId);

        return Result.Success();
    }
}
