using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using FluentAssertions;

namespace Bookify.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        var user = new User("John", "Doe", "john@test.com", "hash123", UserRole.Customer, "+1234567890");

        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Email.Should().Be("john@test.com");
        user.PasswordHash.Should().Be("hash123");
        user.Role.Should().Be(UserRole.Customer);
        user.PhoneNumber.Should().Be("+1234567890");
        user.IsBiometricEnabled.Should().BeFalse();
        user.PreferredLanguage.Should().Be("en");
        user.PreferredCurrency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_EmailIsLowercasedAndTrimmed()
    {
        var user = new User("John", "Doe", "  JOHN@TEST.COM  ", "hash");

        user.Email.Should().Be("john@test.com");
    }

    [Fact]
    public void SetName_EmptyFirstName_ThrowsArgumentException()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");

        var act = () => user.SetName("", "Smith");

        act.Should().Throw<ArgumentException>().WithMessage("*First name*");
    }

    [Fact]
    public void SetName_EmptyLastName_ThrowsArgumentException()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");

        var act = () => user.SetName("John", "");

        act.Should().Throw<ArgumentException>().WithMessage("*Last name*");
    }

    [Fact]
    public void SetName_ValidNames_UpdatesAndTouches()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");
        var originalUpdated = user.UpdatedAt;

        user.SetName("Jane", "Smith");

        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.UpdatedAt.Should().BeAfter(originalUpdated);
    }

    [Fact]
    public void FullName_ReturnsConcatenatedName()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");

        user.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void ChangePassword_WithValidHash_Updates()
    {
        var user = new User("John", "Doe", "john@test.com", "oldhash");

        user.ChangePassword("newhash123");

        user.PasswordHash.Should().Be("newhash123");
    }

    [Fact]
    public void ChangePassword_WithEmptyHash_Throws()
    {
        var user = new User("John", "Doe", "john@test.com", "oldhash");

        var act = () => user.ChangePassword("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToggleBiometric_UpdatesFlag()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");

        user.ToggleBiometric(true);
        user.IsBiometricEnabled.Should().BeTrue();

        user.ToggleBiometric(false);
        user.IsBiometricEnabled.Should().BeFalse();
    }

    [Fact]
    public void RecordLogin_SetsLastLoginAt()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");

        user.RecordLogin();

        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Suspend_SetsSuspendedFlags()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");

        user.Suspend("Violation of terms");

        user.IsSuspended.Should().BeTrue();
        user.SuspensionReason.Should().Be("Violation of terms");
        user.SuspendedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Unsuspend_ClearsSuspendedFlags()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");
        user.Suspend();

        user.Unsuspend();

        user.IsSuspended.Should().BeFalse();
        user.SuspensionReason.Should().BeNull();
        user.SuspendedAt.Should().BeNull();
    }

    [Fact]
    public void SetRole_UpdatesRole()
    {
        var user = new User("John", "Doe", "john@test.com", "hash", UserRole.Customer);

        user.SetRole(UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void SetAvatar_WithUrl_Updates()
    {
        var user = new User("John", "Doe", "john@test.com", "hash");

        user.SetAvatar("https://example.com/avatar.jpg");

        user.AvatarUrl.Should().Be("https://example.com/avatar.jpg");
    }
}
