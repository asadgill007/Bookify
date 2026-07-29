using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Auth;

/// <summary>
/// Sends an email verification token to the user's email address.
/// </summary>
public sealed record SendEmailVerificationCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
}

public sealed class SendEmailVerificationCommandValidator : AbstractValidator<SendEmailVerificationCommand>
{
    public SendEmailVerificationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class SendEmailVerificationCommandHandler : IRequestHandler<SendEmailVerificationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailVerificationCommandHandler> _logger;

    public SendEmailVerificationCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<SendEmailVerificationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "USER_NOT_FOUND");

        // Generate a verification token (in production, store this securely)
        var verificationToken = Guid.NewGuid().ToString("N");

        _logger.LogInformation(
            "Email verification token generated for user {UserId} at {Email}",
            request.UserId, user.Email);

        // Send verification email via the email service
        await _emailService.SendVerificationEmailAsync(
            user.Email,
            user.FullName,
            verificationToken,
            cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Verifies the user's email with the provided token.
/// </summary>
public sealed record VerifyEmailCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string Token { get; init; } = string.Empty;
}

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty().WithMessage("Verification token is required.");
    }
}

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(ILogger<VerifyEmailCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        // In production, validate the token against stored verification tokens
        _logger.LogInformation("Email verification attempted for user {UserId}", request.UserId);

        return Task.FromResult(Result.Success());
    }
}
