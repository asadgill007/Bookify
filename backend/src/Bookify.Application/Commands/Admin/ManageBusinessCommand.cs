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

        business.Verify();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin {AdminId} verified business {BusinessId} ({Name})",
            request.AdminUserId, request.BusinessId, business.Name);

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
