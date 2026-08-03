using Bookify.Application.Commands.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Bookify.Application.Tests;

public class GoogleLoginCommandValidatorTests
{
    private readonly GoogleLoginCommandValidator _sut = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new GoogleLoginCommand { IdToken = "valid.google.id.token" };

        var result = _sut.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void EmptyOrNullIdToken_FailsValidation(string? idToken)
    {
        var command = new GoogleLoginCommand { IdToken = idToken! };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IdToken);
    }

    [Theory]
    [InlineData("customer")]
    [InlineData("provider")]
    [InlineData("businessOwner")]
    [InlineData("CUSTOMER")]
    [InlineData(null)]
    [InlineData("")]
    public void ValidOrEmptyAccountType_PassesValidation(string? accountType)
    {
        var command = new GoogleLoginCommand
        {
            IdToken = "valid.google.id.token",
            AccountType = accountType
        };

        var result = _sut.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("seller")]
    [InlineData("business-owner")]
    [InlineData("partner")]
    public void UnsupportedAccountType_FailsValidation(string accountType)
    {
        var command = new GoogleLoginCommand
        {
            IdToken = "valid.google.id.token",
            AccountType = accountType
        };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AccountType);
    }
}
