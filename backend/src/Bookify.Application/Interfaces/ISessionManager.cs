namespace Bookify.Application.Interfaces;

/// <summary>
/// Manages user sessions for tracking active logins and enforcing session limits.
/// </summary>
public interface ISessionManager
{
    Task<bool> IsSessionValidAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default);
    Task RegisterSessionAsync(Guid userId, string sessionId, DateTime expiresAt, CancellationToken cancellationToken = default);
    Task InvalidateSessionAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default);
    Task InvalidateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
