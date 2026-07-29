using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Stub session manager. In production, use Redis or database-backed sessions.
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> _logger;

    // In-memory session store for development; replace with Redis in production
    private static readonly Dictionary<Guid, List<SessionEntry>> _sessions = new();

    public SessionManager(ILogger<SessionManager> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsSessionValidAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(userId, out var sessions))
        {
            var valid = sessions.Any(s => s.SessionId == sessionId && s.ExpiresAt > DateTime.UtcNow);
            return Task.FromResult(valid);
        }

        return Task.FromResult(false);
    }

    public Task RegisterSessionAsync(Guid userId, string sessionId, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        if (!_sessions.ContainsKey(userId))
            _sessions[userId] = new List<SessionEntry>();

        _sessions[userId].Add(new SessionEntry(sessionId, expiresAt));
        _logger.LogDebug("Session registered for user {UserId}: {SessionId}", userId, sessionId);
        return Task.CompletedTask;
    }

    public Task InvalidateSessionAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(userId, out var sessions))
        {
            sessions.RemoveAll(s => s.SessionId == sessionId);
            _logger.LogDebug("Session invalidated for user {UserId}: {SessionId}", userId, sessionId);
        }

        return Task.CompletedTask;
    }

    public Task InvalidateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _sessions.Remove(userId);
        _logger.LogDebug("All sessions invalidated for user {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(userId, out var sessions))
        {
            var count = sessions.Count(s => s.ExpiresAt > DateTime.UtcNow);
            return Task.FromResult(count);
        }

        return Task.FromResult(0);
    }

    private record SessionEntry(string SessionId, DateTime ExpiresAt);
}
