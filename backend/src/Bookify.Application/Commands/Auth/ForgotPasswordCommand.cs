using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Application.Validators;
using FluentValidation;
using MediatR;

namespace Bookify.Application.Commands.Auth;

public sealed record ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
}

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).Email();
    }
}

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ForgotPasswordAsync(request.Email, cancellationToken);
    }
}
