using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Services;

// ─── Create Service ─────────────────────────────────────
public sealed record CreateServiceCommand : IRequest<Result<Guid>>
{
    public Guid BusinessId { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DurationMinutes { get; init; }
    public decimal PriceAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Category { get; init; }
    public int DisplayOrder { get; init; }
}

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 1440);
        RuleFor(x => x.PriceAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Category).MaximumLength(100);
    }
}

public sealed class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly IBusinessVerificationService _verificationService;
    private readonly ILogger<CreateServiceCommandHandler> _logger;

    public CreateServiceCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        IBusinessVerificationService verificationService,
        ILogger<CreateServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _verificationService = verificationService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result<Guid>.Failure("Business not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageBusinessAsync(request.UserId, business.Id, cancellationToken))
            return Result<Guid>.Failure("You do not have permission to add services to this business.", "FORBIDDEN");

        var service = new Domain.Entities.Service(
            business.Id,
            request.Name,
            request.DurationMinutes,
            request.PriceAmount,
            request.Currency);

        service.UpdateDetails(request.Description, request.Category, request.DisplayOrder, true);

        await _unitOfWork.Services.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Adding a priced service is a checklist item — re-evaluate.
        await _verificationService.EvaluateAndAutoVerifyAsync(business.Id, cancellationToken);

        _logger.LogInformation("Service {ServiceId} created for business {BusinessId} by {UserId}",
            service.Id, business.Id, request.UserId);

        return Result<Guid>.Success(service.Id);
    }
}

// ─── Update Service ─────────────────────────────────────
public sealed record UpdateServiceCommand : IRequest<Result>
{
    public Guid ServiceId { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DurationMinutes { get; init; }
    public decimal PriceAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Category { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 1440);
        RuleFor(x => x.PriceAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Category).MaximumLength(100);
    }
}

public sealed class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<UpdateServiceCommandHandler> _logger;

    public UpdateServiceCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        ILogger<UpdateServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId, cancellationToken);
        if (service == null)
            return Result.Failure("Service not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageBusinessAsync(request.UserId, service.BusinessId, cancellationToken))
            return Result.Failure("You do not have permission to update this service.", "FORBIDDEN");

        service.SetName(request.Name);
        service.SetDuration(request.DurationMinutes);
        service.SetPrice(request.PriceAmount, request.Currency);
        service.UpdateDetails(request.Description, request.Category, request.DisplayOrder, request.IsActive);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Service {ServiceId} updated by {UserId}", request.ServiceId, request.UserId);
        return Result.Success();
    }
}

// ─── Delete Service ─────────────────────────────────────
public sealed record DeleteServiceCommand : IRequest<Result>
{
    public Guid ServiceId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class DeleteServiceCommandValidator : AbstractValidator<DeleteServiceCommand>
{
    public DeleteServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<DeleteServiceCommandHandler> _logger;

    public DeleteServiceCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        ILogger<DeleteServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId, cancellationToken);
        if (service == null)
            return Result.Failure("Service not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageBusinessAsync(request.UserId, service.BusinessId, cancellationToken))
            return Result.Failure("You do not have permission to delete this service.", "FORBIDDEN");

        service.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Service {ServiceId} deleted by {UserId}", request.ServiceId, request.UserId);
        return Result.Success();
    }
}
