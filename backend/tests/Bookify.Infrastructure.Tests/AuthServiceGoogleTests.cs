using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Authentication;
using Bookify.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Bookify.Infrastructure.Tests;

/// <summary>
/// Tests for Google sign-in flows in <see cref="AuthService"/>: token
/// validation, email verification, account creation, account linking and
/// JWT/refresh-token issuance. The Google validator is stubbed so identity
/// claims are fully controlled by the test.
/// </summary>
public class AuthServiceGoogleTests
{
    private const string Subject = "google-subject-123";
    private const string Email = "john@gmail.com";

    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IEmailService _email = Substitute.For<IEmailService>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IGoogleIdTokenValidator _validator = Substitute.For<IGoogleIdTokenValidator>();
    private readonly ILogger<AuthService> _logger = Substitute.For<ILogger<AuthService>>();
    private readonly AuthService _sut;

    public AuthServiceGoogleTests()
    {
        _sut = new AuthService(_uow, _jwt, _hasher, _email, _cache, _validator, _logger);

        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GoogleUserInfo
            {
                Subject = Subject,
                Email = Email,
                EmailVerified = true,
                Name = "John Doe",
                Picture = "https://example.com/pic.jpg"
            });

        _hasher.Hash(Arg.Any<string>()).Returns("password-hash");

        _jwt.GenerateTokensAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TokenResult
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                ExpiresInSeconds = 900
            });
    }

    private static User CreateLocalUser(string email = Email)
        => new("Jane", "Doe", email, "password-hash", UserRole.Customer);

    [Fact]
    public async Task InvalidGoogleToken_ReturnsInvalidGoogleTokenFailure()
    {
        // The validator returns null for any token it rejects — invalid
        // signature, wrong issuer/audience, expired lifetime — so this also
        // covers the expired-token case.
        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GoogleUserInfo?)null);

        var result = await _sut.LoginWithGoogleAsync("invalid.token", null);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_GOOGLE_TOKEN");
        await _uow.Users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _uow.RefreshTokens.DidNotReceive().AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnverifiedEmail_ReturnsEmailNotVerifiedFailure()
    {
        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GoogleUserInfo
            {
                Subject = Subject,
                Email = Email,
                EmailVerified = false,
                Name = "John Doe"
            });

        var result = await _sut.LoginWithGoogleAsync("token-with-unverified-email", null);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_NOT_VERIFIED");
        await _uow.Users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NewGoogleUser_CreatesCustomerAccount_LinksGoogleAndReturnsTokens()
    {
        _uow.Users.GetByGoogleSubjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _uow.Users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _sut.LoginWithGoogleAsync("valid.token", null);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(Email);
        result.Data.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
        result.Data.Role.Should().Be(UserRole.Customer.ToString());
        result.Data.AccessToken.Should().Be("access-token");
        result.Data.RefreshToken.Should().Be("refresh-token");
        result.Data.ExpiresIn.Should().Be(900);

        // A new account is created, linked to the Google subject and persisted
        // with a welcome notification and default preferences.
        await _uow.Users.Received(1).AddAsync(
            Arg.Is<User>(u =>
                u != null &&
                u.GoogleSubject == Subject &&
                u.Email == Email &&
                u.GoogleName == "John Doe" &&
                u.GooglePictureUrl == "https://example.com/pic.jpg" &&
                u.AvatarUrl == "https://example.com/pic.jpg" &&
                u.Role == UserRole.Customer),
            Arg.Any<CancellationToken>());
        await _uow.UserPreferences.Received(1).AddAsync(Arg.Any<UserPreference>(), Arg.Any<CancellationToken>());
        await _uow.Notifications.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _uow.SaveChangesAsync(Arg.Any<CancellationToken>());

        // A refresh token is issued through the standard pipeline.
        await _uow.RefreshTokens.Received(1).AddAsync(
            Arg.Is<RefreshToken>(rt => rt != null && rt.Token == "refresh-token"),
            Arg.Any<CancellationToken>());
        await _jwt.Received(1).GenerateTokensAsync(
            Arg.Any<Guid>(), Email, UserRole.Customer.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NewGoogleUser_WithProviderAccountType_CreatesProviderAccount()
    {
        _uow.Users.GetByGoogleSubjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _uow.Users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _sut.LoginWithGoogleAsync("valid.token", "provider");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Role.Should().Be(UserRole.Provider.ToString());
        await _uow.Users.Received(1).AddAsync(
            Arg.Is<User>(u => u != null && u.Role == UserRole.Provider),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistingGoogleUser_LogsInWithoutCreatingDuplicate()
    {
        var existing = CreateLocalUser();
        existing.LinkGoogleAccount(Subject, "John Doe", null);
        _uow.Users.GetByGoogleSubjectAsync(Subject, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _sut.LoginWithGoogleAsync("valid.token", null);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UserId.Should().Be(existing.Id);
        result.Data.Role.Should().Be(UserRole.Customer.ToString());

        await _uow.Users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _uow.RefreshTokens.Received(1).AddAsync(
            Arg.Is<RefreshToken>(rt => rt != null && rt.Token == "refresh-token"),
            Arg.Any<CancellationToken>());
        existing.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistingLocalAccount_IsLinkedToGoogleIdentityWithoutDuplicate()
    {
        var local = CreateLocalUser();
        _uow.Users.GetByGoogleSubjectAsync(Subject, Arg.Any<CancellationToken>()).Returns((User?)null);
        _uow.Users.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(local);

        var result = await _sut.LoginWithGoogleAsync("valid.token", null);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UserId.Should().Be(local.Id);

        // No duplicate account created; the existing local account is linked.
        await _uow.Users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        local.GoogleSubject.Should().Be(Subject);
        local.GoogleName.Should().Be("John Doe");
        await _uow.RefreshTokens.Received(1).AddAsync(
            Arg.Is<RefreshToken>(rt => rt != null && rt.Token == "refresh-token"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistingAccountLinkedToDifferentGoogle_IsNotOverwritten()
    {
        var local = CreateLocalUser();
        local.LinkGoogleAccount("other-google-subject", "Other", null);
        _uow.Users.GetByGoogleSubjectAsync(Subject, Arg.Any<CancellationToken>()).Returns((User?)null);
        _uow.Users.GetByEmailAsync(Email, Arg.Any<CancellationToken>()).Returns(local);

        var result = await _sut.LoginWithGoogleAsync("valid.token", null);

        result.IsSuccess.Should().BeTrue();
        local.GoogleSubject.Should().Be("other-google-subject");
    }
}
