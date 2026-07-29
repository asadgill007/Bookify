using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Admin;

public sealed record ChangeUserRoleCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid TargetUserId { get; init; }
    public UserRole NewRole { get; init; }
}

public sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("Invalid role specified.");
    }
}

public sealed class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangeUserRoleCommandHandler> _logger;

    public ChangeUserRoleCommandHandler(IUnitOfWork unitOfWork, ILogger<ChangeUserRoleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.TargetUserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "NOT_FOUND");

        var previousRole = user.Role;
        user.SetRole(request.NewRole);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} changed user {UserId} role from {PreviousRole} to {NewRole}",
            request.AdminUserId, request.TargetUserId, previousRole, request.NewRole);

        return Result.Success();
    }
}

public sealed record SuspendUserCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid TargetUserId { get; init; }
    public string? Reason { get; init; }
}

public sealed class SuspendUserCommandValidator : AbstractValidator<SuspendUserCommand>
{
    public SuspendUserCommandValidator()
    {
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class SuspendUserCommandHandler : IRequestHandler<SuspendUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SuspendUserCommandHandler> _logger;

    public SuspendUserCommandHandler(IUnitOfWork unitOfWork, ILogger<SuspendUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SuspendUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.TargetUserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "NOT_FOUND");

        user.Suspend(request.Reason);
        user.SetSuspendedBy(request.AdminUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} suspended user {UserId}. Reason: {Reason}",
            request.AdminUserId, request.TargetUserId, request.Reason ?? "Not specified");

        return Result.Success();
    }
}

public sealed record UnsuspendUserCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid TargetUserId { get; init; }
}

public sealed class UnsuspendUserCommandValidator : AbstractValidator<UnsuspendUserCommand>
{
    public UnsuspendUserCommandValidator()
    {
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
    }
}

public sealed class UnsuspendUserCommandHandler : IRequestHandler<UnsuspendUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnsuspendUserCommandHandler> _logger;

    public UnsuspendUserCommandHandler(IUnitOfWork unitOfWork, ILogger<UnsuspendUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UnsuspendUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.TargetUserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "NOT_FOUND");

        user.Unsuspend();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} unsuspended user {UserId}",
            request.AdminUserId, request.TargetUserId);

        return Result.Success();
    }
}

public sealed record DeleteUserCommand : IRequest<Result>
{
    public Guid AdminUserId { get; init; }
    public Guid TargetUserId { get; init; }
}

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
    }
}

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.TargetUserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "NOT_FOUND");

        // Prevent self-deletion
        if (request.AdminUserId == request.TargetUserId)
            return Result.Failure("Cannot delete your own account.", "INVALID_OPERATION");

        await _unitOfWork.Users.DeleteAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} deleted user {UserId}",
            request.AdminUserId, request.TargetUserId);

        return Result.Success();
    }
}
