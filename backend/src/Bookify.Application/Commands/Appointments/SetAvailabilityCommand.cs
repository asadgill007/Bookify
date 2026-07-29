using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Appointments;

/// <summary>
/// Sets or updates a provider's weekly recurring availability for a specific day.
/// </summary>
public sealed record SetWeeklyAvailabilityCommand : IRequest<Result>
{
    public Guid ProviderId { get; init; }
    public Guid UserId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public int SlotDurationMinutes { get; init; } = 60;
    public bool IsAvailable { get; init; } = true;
}

public sealed class SetWeeklyAvailabilityCommandValidator : AbstractValidator<SetWeeklyAvailabilityCommand>
{
    public SetWeeklyAvailabilityCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime).WithMessage("Start time must be before end time.");
        RuleFor(x => x.SlotDurationMinutes)
            .InclusiveBetween(15, 480).WithMessage("Slot duration must be between 15 and 480 minutes.");
    }
}

public sealed class SetWeeklyAvailabilityCommandHandler : IRequestHandler<SetWeeklyAvailabilityCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProviderRepository _providerRepository;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<SetWeeklyAvailabilityCommandHandler> _logger;

    public SetWeeklyAvailabilityCommandHandler(
        IUnitOfWork unitOfWork,
        IProviderRepository providerRepository,
        IPermissionService permissionService,
        ILogger<SetWeeklyAvailabilityCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _providerRepository = providerRepository;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<Result> Handle(SetWeeklyAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null)
            return Result.Failure("Provider not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageProviderAsync(request.UserId, request.ProviderId, cancellationToken))
            return Result.Failure("You do not have permission to manage this provider.", "FORBIDDEN");

        var availability = new ProviderAvailability(
            request.ProviderId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            request.SlotDurationMinutes);

        if (!request.IsAvailable)
            availability.ToggleAvailable(false);

        // Use repository to add the availability directly
        // Note: In production, consider updating existing records instead of always creating new ones
        await _providerRepository.AddAvailabilityAsync(availability, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Availability set for provider {ProviderId} on {DayOfWeek}: {Start}-{End}, duration={Duration}min",
            request.ProviderId, request.DayOfWeek, request.StartTime, request.EndTime, request.SlotDurationMinutes);

        return Result.Success();
    }
}

/// <summary>
/// Adds a date-specific availability override (holiday, leave, extra hours).
/// </summary>
public sealed record AddAvailabilityOverrideCommand : IRequest<Result>
{
    public Guid ProviderId { get; init; }
    public Guid UserId { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public bool IsAvailable { get; init; } = true;
    public string? Reason { get; init; }
}

public sealed class AddAvailabilityOverrideCommandValidator : AbstractValidator<AddAvailabilityOverrideCommand>
{
    public AddAvailabilityOverrideCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Override date must be today or in the future.");
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}

public sealed class AddAvailabilityOverrideCommandHandler : IRequestHandler<AddAvailabilityOverrideCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProviderRepository _providerRepository;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<AddAvailabilityOverrideCommandHandler> _logger;

    public AddAvailabilityOverrideCommandHandler(
        IUnitOfWork unitOfWork,
        IProviderRepository providerRepository,
        IPermissionService permissionService,
        ILogger<AddAvailabilityOverrideCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _providerRepository = providerRepository;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<Result> Handle(AddAvailabilityOverrideCommand request, CancellationToken cancellationToken)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null)
            return Result.Failure("Provider not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageProviderAsync(request.UserId, request.ProviderId, cancellationToken))
            return Result.Failure("You do not have permission to manage this provider.", "FORBIDDEN");

        ProviderAvailabilityOverride overrideEntry;

        if (request.StartTime.HasValue && request.EndTime.HasValue)
        {
            overrideEntry = new ProviderAvailabilityOverride(
                request.ProviderId,
                request.Date,
                request.StartTime.Value,
                request.EndTime.Value,
                request.Reason);
        }
        else
        {
            overrideEntry = new ProviderAvailabilityOverride(
                request.ProviderId,
                request.Date,
                request.IsAvailable,
                request.Reason);
        }

        // Use repository to add the override directly (avoids navigation collection issues)
        await _providerRepository.AddAvailabilityOverrideAsync(overrideEntry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var status = request.IsAvailable ? "available" : "unavailable";
        _logger.LogInformation(
            "Availability override added for provider {ProviderId} on {Date}: {Status} - {Reason}",
            request.ProviderId, request.Date, status, request.Reason ?? "N/A");

        return Result.Success();
    }
}
