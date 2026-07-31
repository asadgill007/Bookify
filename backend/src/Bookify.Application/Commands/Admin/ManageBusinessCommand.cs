using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Admin;

public sealed record VerifyBusinessCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid BusinessId { get; init; }
}

public sealed class VerifyBusinessCommandHandler : IRequestHandler<VerifyBusinessCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyBusinessCommandHandler> _logger;

    public VerifyBusinessCommandHandler(IUnitOfWork unitOfWork, ILogger<VerifyBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(VerifyBusinessCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        business.Verify(request.AdminUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin {AdminId} verified business {BusinessId} ({Name})",
            request.AdminUserId, request.BusinessId, business.Name);

        return Result.Success();
    }
}

public sealed record RejectBusinessCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid BusinessId { get; init; }
    public string? Reason { get; init; }
}

public sealed class RejectBusinessCommandValidator : AbstractValidator<RejectBusinessCommand>
{
    public RejectBusinessCommandValidator()
    {
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().WithMessage("A rejection reason is required.")
            .MaximumLength(1000);
    }
}

public sealed class RejectBusinessCommandHandler : IRequestHandler<RejectBusinessCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectBusinessCommandHandler> _logger;

    public RejectBusinessCommandHandler(IUnitOfWork unitOfWork, ILogger<RejectBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RejectBusinessCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        business.Reject(request.Reason, request.AdminUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin {AdminId} rejected business {BusinessId} ({Name}): {Reason}",
            request.AdminUserId, request.BusinessId, business.Name, request.Reason);

        return Result.Success();
    }
}

public sealed record ResubmitBusinessCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public Guid BusinessId { get; init; }
}

public sealed class ResubmitBusinessCommandValidator : AbstractValidator<ResubmitBusinessCommand>
{
    public ResubmitBusinessCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BusinessId).NotEmpty();
    }
}

public sealed class ResubmitBusinessCommandHandler : IRequestHandler<ResubmitBusinessCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<ResubmitBusinessCommandHandler> _logger;

    public ResubmitBusinessCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        ILogger<ResubmitBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResubmitBusinessCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        if (!await _permissionService.CanManageBusinessAsync(request.UserId, business.Id, cancellationToken))
            return Result.Failure("You do not have permission to update this business.", "FORBIDDEN");

        if (business.VerificationStatus != Domain.Enums.VerificationStatus.Rejected)
            return Result.Failure("Only rejected businesses can be resubmitted for review.", "INVALID_STATE");

        business.ResubmitForReview();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Business {BusinessId} resubmitted for review by {UserId}", business.Id, request.UserId);
        return Result.Success();
    }
}

public sealed record ToggleBusinessActiveCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid BusinessId { get; init; }
    public bool IsActive { get; init; }
}

public sealed class ToggleBusinessActiveCommandHandler : IRequestHandler<ToggleBusinessActiveCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ToggleBusinessActiveCommandHandler> _logger;

    public ToggleBusinessActiveCommandHandler(IUnitOfWork unitOfWork, ILogger<ToggleBusinessActiveCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ToggleBusinessActiveCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result.Failure("Business not found.", "NOT_FOUND");

        business.ToggleActive(request.IsActive);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var action = request.IsActive ? "activated" : "deactivated";
        _logger.LogInformation("Admin {AdminId} {Action} business {BusinessId}",
            request.AdminUserId, action, request.BusinessId);

        return Result.Success();
    }
}
