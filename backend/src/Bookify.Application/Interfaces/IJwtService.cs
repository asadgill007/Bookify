using System.Security.Claims;

namespace Bookify.Application.Interfaces;

public class TokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public int ExpiresInSeconds { get; set; }
}

public interface IJwtService
{
    Task<TokenResult> GenerateTokensAsync(Guid userId, string email, string role, CancellationToken cancellationToken = default);
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    string GenerateRefreshToken();
}
