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
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string TimeZone { get; init; } = "UTC";
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Website { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
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
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class CreateBusinessCommandHandler : IRequestHandler<CreateBusinessCommand, Result<BusinessCreatedResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBusinessCommandHandler> _logger;

    public CreateBusinessCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BusinessCreatedResult>> Handle(CreateBusinessCommand request, CancellationToken cancellationToken)
    {
        var slug = request.Name.ToLowerInvariant().Replace(" ", "-");

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

        business.UpdateDetails(request.Description, request.Email, request.PhoneNumber, request.Website, null);

        if (request.Latitude.HasValue && request.Longitude.HasValue)
            business.SetGeoLocation(request.Latitude.Value, request.Longitude.Value);

        await _unitOfWork.Businesses.AddAsync(business, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} created business {BusinessId} ({Name})", request.UserId, business.Id, request.Name);

        return Result<BusinessCreatedResult>.Success(new BusinessCreatedResult
        {
            Id = business.Id,
            Slug = business.Slug
        });
    }
}
