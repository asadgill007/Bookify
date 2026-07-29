using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public string JwtId { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string token, string jwtId, DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        JwtId = jwtId;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsUsed && !IsRevoked && !IsExpired;

    public void MarkAsUsed()
    {
        IsUsed = true;
        Touch();
    }

    public void Revoke()
    {
        IsRevoked = true;
        Touch();
    }
}
