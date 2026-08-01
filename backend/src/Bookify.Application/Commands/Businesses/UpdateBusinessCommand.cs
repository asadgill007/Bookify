using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Businesses;

public sealed record UpdateBusinessCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public Guid BusinessId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string AddressLine1 { get; init; } = string.Empty;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string? State { get; init; }
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string TimeZone { get; init; } = "UTC";
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Website { get; init; }
    public string? CancellationPolicy { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? BookingType { get; init; }
    public IReadOnlyList<Guid> CategoryIds { get; init; } = Array.Empty<Guid>();
}

public sealed class UpdateBusinessCommandValidator : AbstractValidator<UpdateBusinessCommand>
{
    public UpdateBusinessCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Website).MaximumLength(500);
        RuleFor(x => x.CancellationPolicy).MaximumLength(2000);
        RuleFor(x => x.CategoryIds).NotNull();
    }
}

public sealed class UpdateBusinessCommandHandler : IRequestHandler<UpdateBusinessCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly IBusinessVerificationService _verificationService;
    private readonly ILogger<UpdateBusinessCommandHandler> _logger;

    public UpdateBusinessCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        IBusinessVerificationService verificationService,
        ILogger<UpdateBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _verificationService = verificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateBusinessCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageBusinessAsync(request.UserId, business.Id, cancellationToken))
            return Result.Failure("You do not have permission to update this business.", "FORBIDDEN");

        var slug = CreateBusinessCommandHandler.Slugify(request.Name);
        business.SetName(request.Name, slug);
        business.UpdateAddress(request.AddressLine1, request.AddressLine2, request.City, request.State, request.PostalCode, request.Country);
        business.UpdateDetails(request.Description, request.Email, request.PhoneNumber, request.Website, request.CancellationPolicy);

        if (request.Latitude.HasValue && request.Longitude.HasValue)
            business.SetGeoLocation(request.Latitude.Value, request.Longitude.Value);

        if (!string.IsNullOrWhiteSpace(request.BookingType) &&
            Enum.TryParse<BookingType>(request.BookingType, true, out var bookingType))
        {
            business.SetBookingType(bookingType);
        }

        // Replace category links (only remove/add the delta to avoid unique-index conflicts
        // between soft-deleted and new rows on the (BusinessId, CategoryId) index).
        var existingCategoryIds = business.BusinessCategories.Select(bc => bc.CategoryId).ToHashSet();
        var targetCategoryIds = request.CategoryIds.ToHashSet();

        foreach (var bc in business.BusinessCategories.Where(bc => !targetCategoryIds.Contains(bc.CategoryId)).ToList())
        {
            bc.SoftDelete();
        }

        foreach (var categoryId in targetCategoryIds.Except(existingCategoryIds))
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
            if (category == null)
                return Result.Failure($"Category '{categoryId}' does not exist.", "CATEGORY_NOT_FOUND");

            business.BusinessCategories.Add(new BusinessCategory(business.Id, categoryId));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-evaluate the checklist after every save so the business goes live
        // automatically the moment all required info is present.
        await _verificationService.EvaluateAndAutoVerifyAsync(business.Id, cancellationToken);

        _logger.LogInformation("Business {BusinessId} updated by {UserId}", business.Id, request.UserId);
        return Result.Success();
    }
}
