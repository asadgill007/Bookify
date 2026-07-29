using System.Text.Json;
using Bookify.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// <see cref="ICacheService"/> implementation backed by <see cref="IDistributedCache"/>.
/// Supports Redis, SQL Server, and other distributed cache providers.
/// To enable Redis:
/// 1. Install package: dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
/// 2. Add in Program.cs: builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = "..."; });
/// 3. Update DI registration: services.AddSingleton&lt;ICacheService, DistributedCacheService&gt;();
/// </summary>
public sealed class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        if (bytes == null)
        {
            _logger.LogTrace("Cache MISS for key {CacheKey}", key);
            return null;
        }

        _logger.LogTrace("Cache HIT for key {CacheKey}", key);
        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        var entryOptions = (options ?? CacheEntryOptions.Default).ToDistributedCacheEntryOptions();
        await _cache.SetAsync(key, bytes, entryOptions, cancellationToken);
        _logger.LogTrace("Cache SET for key {CacheKey}", key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
        _logger.LogTrace("Cache REMOVED for key {CacheKey}", key);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        if (bytes != null)
        {
            _logger.LogTrace("Cache HIT for key {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)!;
        }

        _logger.LogTrace("Cache MISS for key {CacheKey}, executing factory", key);
        var value = await factory();

        var entryOptions = (options ?? CacheEntryOptions.Default).ToDistributedCacheEntryOptions();
        var serialized = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await _cache.SetAsync(key, serialized, entryOptions, cancellationToken);

        return value;
    }
}
