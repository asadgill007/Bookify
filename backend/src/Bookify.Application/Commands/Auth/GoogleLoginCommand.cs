using Bookify.Application.Common;
using Bookify.Application.DTOs.Auth;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Bookify.Application.Commands.Auth;

public sealed record GoogleLoginCommand : IRequest<Result<AuthResponse>>
{
    public string IdToken { get; init; } = string.Empty;
    public string? AccountType { get; init; }
}

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().WithMessage("Google ID token is required.");
        RuleFor(x => x.AccountType)
            .Must(t => string.IsNullOrWhiteSpace(t) ||
                       new[] { "customer", "provider", "businessowner" }
                           .Contains(t.Trim().ToLowerInvariant()))
            .WithMessage("Account type must be one of: customer, provider, businessOwner.");
    }
}

public sealed class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResponse>>
{
    private readonly IAuthService _authService;

    public GoogleLoginCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<Result<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginWithGoogleAsync(request.IdToken, request.AccountType, cancellationToken);
    }
}
