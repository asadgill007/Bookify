using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Businesses;

public sealed record CreateBusinessCommand : IRequest<Result<BusinessCreatedResult>>
{
    public Guid UserId { get; init; }
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
    public string? CoverImageUrl { get; init; }

    /// <summary>Category IDs selected from the real categories list.</summary>
    public IReadOnlyList<Guid> CategoryIds { get; init; } = Array.Empty<Guid>();
}

public sealed class BusinessCreatedResult
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
}

public sealed class CreateBusinessCommandValidator : AbstractValidator<CreateBusinessCommand>
{
    public CreateBusinessCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
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

public sealed class CreateBusinessCommandHandler : IRequestHandler<CreateBusinessCommand, Result<BusinessCreatedResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessVerificationService _verificationService;
    private readonly ILogger<CreateBusinessCommandHandler> _logger;

    public CreateBusinessCommandHandler(
        IUnitOfWork unitOfWork,
        IBusinessVerificationService verificationService,
        ILogger<CreateBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _verificationService = verificationService;
        _logger = logger;
    }

    public async Task<Result<BusinessCreatedResult>> Handle(CreateBusinessCommand request, CancellationToken cancellationToken)
    {
        var slug = Slugify(request.Name);

        if (await _unitOfWork.Businesses.GetBySlugAsync(slug, cancellationToken) != null)
            return Result<BusinessCreatedResult>.Failure("A business with this name already exists.", "SLUG_CONFLICT");

        var business = new Business(
            request.UserId,
            request.Name,
            slug,
            request.AddressLine1,
            request.City,
            request.PostalCode,
            request.Country,
            request.TimeZone,
            request.Currency);

        business.UpdateAddress(request.AddressLine1, request.AddressLine2, request.City, request.State, request.PostalCode, request.Country);
        business.UpdateDetails(request.Description, request.Email, request.PhoneNumber, request.Website, request.CancellationPolicy);

        if (request.Latitude.HasValue && request.Longitude.HasValue)
            business.SetGeoLocation(request.Latitude.Value, request.Longitude.Value);

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
            business.SetImages(request.CoverImageUrl, null);

        // Link the selected categories (from the real categories list)
        foreach (var categoryId in request.CategoryIds)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
            if (category == null)
                return Result<BusinessCreatedResult>.Failure($"Category '{categoryId}' does not exist.", "CATEGORY_NOT_FOUND");

            business.BusinessCategories.Add(new BusinessCategory(business.Id, categoryId));
        }

        await _unitOfWork.Businesses.AddAsync(business, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auto-verification: if the listing is already complete it goes live
        // immediately without needing an admin.
        var checklist = await _verificationService.EvaluateAndAutoVerifyAsync(business.Id, cancellationToken);
        if (checklist.IsComplete)
            _logger.LogInformation("Business {BusinessId} auto-verified on creation.", business.Id);

        _logger.LogInformation("User {UserId} created business {BusinessId} ({Name})", request.UserId, business.Id, request.Name);

        return Result<BusinessCreatedResult>.Success(new BusinessCreatedResult
        {
            Id = business.Id,
            Slug = business.Slug
        });
    }

    internal static string Slugify(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        var builder = new System.Text.StringBuilder();
        foreach (var c in slug)
        {
            if (char.IsLetterOrDigit(c))
                builder.Append(c);
            else if (c == ' ')
                builder.Append('-');
        }
        return builder.ToString().Trim('-');
    }
}
