using Bookify.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Extension methods to convert <see cref="CacheEntryOptions"/> into framework-specific cache entry options.
/// These live in the Infrastructure layer to avoid coupling the Application layer to caching abstractions.
/// </summary>
internal static class CacheEntryOptionsExtensions
{
    public static MemoryCacheEntryOptions ToMemoryCacheEntryOptions(this CacheEntryOptions options)
    {
        var result = new MemoryCacheEntryOptions();
        if (options.AbsoluteExpirationRelativeToNow.HasValue)
            result.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
        if (options.SlidingExpiration.HasValue)
            result.SlidingExpiration = options.SlidingExpiration;
        return result;
    }

    public static DistributedCacheEntryOptions ToDistributedCacheEntryOptions(this CacheEntryOptions options)
    {
        var result = new DistributedCacheEntryOptions();
        if (options.AbsoluteExpirationRelativeToNow.HasValue)
            result.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
        if (options.SlidingExpiration.HasValue)
            result.SlidingExpiration = options.SlidingExpiration;
        return result;
    }
}
