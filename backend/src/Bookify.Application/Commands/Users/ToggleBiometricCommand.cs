using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Users;

public sealed record ToggleBiometricCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public bool Enabled { get; init; }
}

public sealed class ToggleBiometricCommandValidator : AbstractValidator<ToggleBiometricCommand>
{
    public ToggleBiometricCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class ToggleBiometricCommandHandler : IRequestHandler<ToggleBiometricCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ToggleBiometricCommandHandler> _logger;

    public ToggleBiometricCommandHandler(IUnitOfWork unitOfWork, ILogger<ToggleBiometricCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ToggleBiometricCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "NOT_FOUND");

        user.ToggleBiometric(request.Enabled);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} set biometric to {Enabled}", request.UserId, request.Enabled);
        return Result.Success();
    }
}
