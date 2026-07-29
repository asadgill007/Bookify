using Bookify.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// <see cref="ICacheService"/> implementation backed by <see cref="IMemoryCache"/>.
/// Suitable for single-instance deployments and development.
/// For multi-instance or production, switch to <see cref="DistributedCacheService"/>.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var hit = _cache.TryGetValue(key, out T? value);
        if (hit)
            _logger.LogTrace("Cache HIT for key {CacheKey}", key);
        else
            _logger.LogTrace("Cache MISS for key {CacheKey}", key);

        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        var entryOptions = (options ?? CacheEntryOptions.Default).ToMemoryCacheEntryOptions();
        _cache.Set(key, value, entryOptions);
        _logger.LogTrace("Cache SET for key {CacheKey}", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        _logger.LogTrace("Cache REMOVED for key {CacheKey}", key);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out T? cached))
        {
            _logger.LogTrace("Cache HIT for key {CacheKey}", key);
            return cached!;
        }

        _logger.LogTrace("Cache MISS for key {CacheKey}, executing factory", key);
        var value = await factory();

        var entryOptions = (options ?? CacheEntryOptions.Default).ToMemoryCacheEntryOptions();
        _cache.Set(key, value, entryOptions);

        return value;
    }
}
