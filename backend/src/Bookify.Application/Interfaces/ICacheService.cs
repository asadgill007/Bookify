namespace Bookify.Application.Interfaces;

/// <summary>
/// Options for cache entry expiration.
/// </summary>
public class CacheEntryOptions
{
    /// <summary>Absolute expiration time from now.</summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>Sliding expiration window (resets on each access).</summary>
    public TimeSpan? SlidingExpiration { get; set; }

    public static CacheEntryOptions Default => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };

    public static CacheEntryOptions ShortLived => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
        SlidingExpiration = TimeSpan.FromSeconds(30)
    };

    public static CacheEntryOptions LongLived => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        SlidingExpiration = TimeSpan.FromMinutes(10)
    };

    public static CacheEntryOptions Statistics => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(1)
    };
}

/// <summary>
/// Abstraction for in-memory and distributed caching.
/// </summary>
public interface ICacheService
{
    /// <summary>Get a cached value by key, or null if not found.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>Set a cached value with expiration options.</summary>
    Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>Remove a cached value by key.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Get a cached value or create it if not found (cache-aside pattern).</summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Static cache key constants to prevent magic strings.
/// </summary>
public static class CacheKeys
{
    public static string AdminDashboard => "admin:dashboard";
    public static string BusinessDashboard(Guid businessId) => $"business:{businessId}:dashboard";
    public static string BusinessReviews(Guid businessId, int page, int pageSize) => $"business:{businessId}:reviews:{page}:{pageSize}";
    public static string ProviderReviews(Guid providerId, int page, int pageSize) => $"provider:{providerId}:reviews:{page}:{pageSize}";
    public static string ReviewStatistics(Guid businessId) => $"business:{businessId}:reviewstats";
    public static string TopRatedProviders(int count) => $"providers:toprated:{count}";
    public static string Categories => "categories";
    public static string BusinessBySlug(string slug) => $"business:slug:{slug}";
    public static string ProviderSlots(Guid providerId, DateOnly date) => $"provider:{providerId}:slots:{date:yyyy-MM-dd}";
    public static string BusinessSearch(string search, int page, int pageSize) => $"business:search:{search}:{page}:{pageSize}";
}
