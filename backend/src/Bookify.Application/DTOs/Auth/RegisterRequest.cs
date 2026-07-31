using Bookify.Application.Validators;
using FluentValidation;

namespace Bookify.Application.DTOs.Auth;

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Account type: "customer" (default), "provider", or "businessOwner".
    /// "admin" is never allowed through public registration.
    /// </summary>
    public string AccountType { get; set; } = "customer";
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email).Email();

        RuleFor(x => x.Password).Password();

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.AccountType)
            .Must(BeValidAccountType).WithMessage("Account type must be one of: customer, provider, businessOwner.");
    }

    private static bool BeValidAccountType(string accountType)
    {
        return string.IsNullOrWhiteSpace(accountType) ||
               new[] { "customer", "provider", "businessowner" }
                   .Contains(accountType.Trim().ToLowerInvariant());
    }
}
