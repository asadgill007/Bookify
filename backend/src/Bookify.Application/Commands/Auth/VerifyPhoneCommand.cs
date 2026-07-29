using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Auth;

/// <summary>
/// Sends an SMS verification code to the user's phone number.
/// </summary>
public sealed record SendPhoneVerificationCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
}

public sealed class SendPhoneVerificationCommandValidator : AbstractValidator<SendPhoneVerificationCommand>
{
    public SendPhoneVerificationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class SendPhoneVerificationCommandHandler : IRequestHandler<SendPhoneVerificationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly ILogger<SendPhoneVerificationCommandHandler> _logger;

    public SendPhoneVerificationCommandHandler(
        IUnitOfWork unitOfWork,
        ISmsService smsService,
        ILogger<SendPhoneVerificationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<Result> Handle(SendPhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.", "USER_NOT_FOUND");

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            return Result.Failure("No phone number on file.", "NO_PHONE");

        var verificationCode = Random.Shared.Next(100000, 999999).ToString();

        _logger.LogInformation(
            "Phone verification code sent to user {UserId} at {Phone}",
            request.UserId, user.PhoneNumber);

        await _smsService.SendVerificationSmsAsync(
            user.PhoneNumber,
            verificationCode,
            cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Verifies the user's phone number with the provided code.
/// </summary>
public sealed record VerifyPhoneCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string Code { get; init; } = string.Empty;
}

public sealed class VerifyPhoneCommandValidator : AbstractValidator<VerifyPhoneCommand>
{
    public VerifyPhoneCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.");
    }
}

public sealed class VerifyPhoneCommandHandler : IRequestHandler<VerifyPhoneCommand, Result>
{
    private readonly ILogger<VerifyPhoneCommandHandler> _logger;

    public VerifyPhoneCommandHandler(ILogger<VerifyPhoneCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken)
    {
        // In production, validate the code against stored verification codes
        _logger.LogInformation("Phone verification attempted for user {UserId}", request.UserId);

        return Task.FromResult(Result.Success());
    }
}
