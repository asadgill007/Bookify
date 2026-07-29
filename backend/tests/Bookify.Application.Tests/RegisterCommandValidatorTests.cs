using Bookify.Application.Commands.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Bookify.Application.Tests;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _sut = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "SecureP@ss1",
            ConfirmPassword = "SecureP@ss1"
        };

        var result = _sut.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyFirstName_FailsValidation(string? firstName)
    {
        var command = new RegisterCommand
        {
            FirstName = firstName!,
            LastName = "Doe",
            Email = "john@example.com",
            Password = "SecureP@ss1",
            ConfirmPassword = "SecureP@ss1"
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void InvalidEmail_FailsValidation()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "not-an-email",
            Password = "SecureP@ss1",
            ConfirmPassword = "SecureP@ss1"
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void WeakPassword_FailsValidation()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "weak",
            ConfirmPassword = "weak"
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void MismatchedPasswords_FailsValidation()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "SecureP@ss1",
            ConfirmPassword = "DifferentP@ss1"
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Passwords do not match.");
    }

    [Fact]
    public void FirstNameTooLong_FailsValidation()
    {
        var command = new RegisterCommand
        {
            FirstName = new string('a', 101),
            LastName = "Doe",
            Email = "john@example.com",
            Password = "SecureP@ss1",
            ConfirmPassword = "SecureP@ss1"
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }
}
