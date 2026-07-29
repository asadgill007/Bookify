using Bookify.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Bookify.Infrastructure.Tests;

public class JwtServiceTests
{
    private readonly JwtService _sut;

    public JwtServiceTests()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsASecretKeyThatIsAtLeast32BytesLong!ABCD",
            ["Jwt:Issuer"] = "Bookify",
            ["Jwt:Audience"] = "BookifyApp",
            ["Jwt:ExpiresInSeconds"] = "900"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _sut = new JwtService(configuration);
    }

    [Fact]
    public async Task GenerateTokensAsync_ReturnsNonNullTokens()
    {
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "Customer";

        var result = await _sut.GenerateTokensAsync(userId, email, role);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresInSeconds.Should().Be(900);
    }

    [Fact]
    public async Task GenerateTokensAsync_DifferentUsers_ReturnsDifferentTokens()
    {
        var result1 = await _sut.GenerateTokensAsync(Guid.NewGuid(), "user1@test.com", "Customer");
        var result2 = await _sut.GenerateTokensAsync(Guid.NewGuid(), "user2@test.com", "Customer");

        result1.AccessToken.Should().NotBe(result2.AccessToken);
        result1.RefreshToken.Should().NotBe(result2.RefreshToken);
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_ReturnsPrincipal()
    {
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "Customer";

        var tokens = await _sut.GenerateTokensAsync(userId, email, role);

        var principal = await _sut.ValidateTokenAsync(tokens.AccessToken);

        principal.Should().NotBeNull();
        principal!.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value.Should().Be(role);
        principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value.Should().Be(email);
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidToken_ReturnsNull()
    {
        var principal = await _sut.ValidateTokenAsync("invalid-token-that-cannot-be-validated");

        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_ExpiredToken_ReturnsNull()
    {
        // Create a config with 0-second expiry
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsASecretKeyThatIsAtLeast32BytesLong!ABCD",
            ["Jwt:Issuer"] = "Bookify",
            ["Jwt:Audience"] = "BookifyApp",
            ["Jwt:ExpiresInSeconds"] = "0"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var expiredService = new JwtService(configuration);

        var tokens = await expiredService.GenerateTokensAsync(Guid.NewGuid(), "test@test.com", "Customer");

        var principal = await expiredService.ValidateTokenAsync(tokens.AccessToken);

        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateRefreshToken_Returns64ByteBase64String()
    {
        var token = _sut.GenerateRefreshToken();

        token.Should().NotBeNullOrWhiteSpace();
        // 64 random bytes = 88 base64 chars (including padding)
        Convert.FromBase64String(token).Length.Should().Be(64);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        token1.Should().NotBe(token2);
    }
}
