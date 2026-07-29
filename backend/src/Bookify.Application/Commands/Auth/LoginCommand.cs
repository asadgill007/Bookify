using Bookify.Application.Common;
using Bookify.Application.DTOs.Auth;
using Bookify.Application.Interfaces;
using Bookify.Application.Validators;
using FluentValidation;
using MediatR;

namespace Bookify.Application.Commands.Auth;

public sealed record LoginCommand : IRequest<Result<AuthResponse>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).Email();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest
        {
            Email = request.Email,
            Password = request.Password
        };

        return await _authService.LoginAsync(loginRequest, cancellationToken);
    }
}
