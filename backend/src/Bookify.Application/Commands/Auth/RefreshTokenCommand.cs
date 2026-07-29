using Bookify.Application.Common;
using Bookify.Application.DTOs.Auth;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Bookify.Application.Commands.Auth;

public sealed record RefreshTokenCommand : IRequest<Result<AuthResponse>>
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Access token is required.");
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenRequest = new RefreshTokenRequest
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken
        };

        return await _authService.RefreshTokenAsync(tokenRequest, cancellationToken);
    }
}
