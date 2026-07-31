using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Application.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Providers;

public sealed record AddBusinessProviderCommand : IRequest<Result<BusinessProviderResult>>
{
    public Guid BusinessId { get; init; }
    public Guid OwnerUserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public int DisplayOrder { get; init; }
    /// <summary>Optional service IDs this provider offers.</summary>
    public IReadOnlyList<Guid> ServiceIds { get; init; } = Array.Empty<Guid>();
}

public sealed class BusinessProviderResult
{
    public Guid ProviderId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class AddBusinessProviderCommandValidator : AbstractValidator<AddBusinessProviderCommand>
{
    public AddBusinessProviderCommandValidator()
    {
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).Email();
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Bio).MaximumLength(1000);
        RuleFor(x => x.AvatarUrl).MaximumLength(1000);
        RuleFor(x => x.ServiceIds).NotNull();
    }
}

public sealed class AddBusinessProviderCommandHandler : IRequestHandler<AddBusinessProviderCommand, Result<BusinessProviderResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<AddBusinessProviderCommandHandler> _logger;

    public AddBusinessProviderCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        IPermissionService permissionService,
        ILogger<AddBusinessProviderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<Result<BusinessProviderResult>> Handle(AddBusinessProviderCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result<BusinessProviderResult>.Failure("Business not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageBusinessAsync(request.OwnerUserId, business.Id, cancellationToken))
            return Result<BusinessProviderResult>.Failure("You do not have permission to add providers to this business.", "FORBIDDEN");

        // Reuse an existing user if the email is already registered, otherwise create a provider account.
        var userResult = await _authService.RegisterStaffAsync(
            request.FirstName, request.LastName, request.Email, request.AvatarUrl, cancellationToken);
        if (userResult.IsFailure)
            return Result<BusinessProviderResult>.Failure(userResult.Error!, userResult.ErrorCode);

        var user = userResult.Data!;

        // Keep the staff member's display name/avatar in sync with the request.
        user.SetName(request.FirstName, request.LastName);
        if (request.AvatarUrl != null)
            user.SetAvatar(request.AvatarUrl);

        var provider = new Domain.Entities.Provider(user.Id, business.Id, request.Title);
        provider.UpdateDetails(request.Title, request.Bio, request.DisplayOrder);

        await _unitOfWork.Providers.AddAsync(provider, cancellationToken);

        // Link provider to the selected services
        foreach (var serviceId in request.ServiceIds)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(serviceId, cancellationToken);
            if (service != null && service.BusinessId == business.Id)
            {
                provider.ProviderServices.Add(new Domain.Entities.ProviderService(provider.Id, serviceId));
            }
        }

        // Seed default weekly availability (Mon-Fri 9am-6pm, Sat 10am-3pm, 60-min slots)
        // so the provider is immediately bookable. Mirrors the seeded provider availability.
        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday })
        {
            provider.Availabilities.Add(new Domain.Entities.ProviderAvailability(
                provider.Id, day, new TimeOnly(9, 0), new TimeOnly(18, 0), 60));
        }
        provider.Availabilities.Add(new Domain.Entities.ProviderAvailability(
            provider.Id, DayOfWeek.Saturday, new TimeOnly(10, 0), new TimeOnly(15, 0), 60));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Provider {ProviderId} (user {UserId}) added to business {BusinessId} by {OwnerUserId}",
            provider.Id, user.Id, business.Id, request.OwnerUserId);

        return Result<BusinessProviderResult>.Success(new BusinessProviderResult
        {
            ProviderId = provider.Id,
            UserId = user.Id
        });
    }
}
