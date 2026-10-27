using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Echo.Shared.Caching;

public sealed class DistributedCacheService(
    IDistributedCache cache,
    ILogger<DistributedCacheService> logger,
    IOptions<CacheOptions> options) : ICacheService
{
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<DistributedCacheService> _logger = logger;
    private readonly CacheOptions _options = options.Value;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default) where T : class
    {
        var cached = await _cache.GetAsync(key, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Cache HIT: {Key}", key);
            return JsonSerializer.Deserialize<T>(cached, _json);
        }

        _logger.LogDebug("Cache MISS: {Key}", key);
        var value = await factory(ct);

        if (value is not null || _options.CacheNulls)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
            await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? _options.DefaultTtl
            }, ct);
        }

        return value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var bytes = await _cache.GetAsync(key, ct);
        var value = bytes is not null ? JsonSerializer.Deserialize<T>(bytes, _json) : null;
        _logger.LogDebug(value is not null ? "Cache HIT: {Key}" : "Cache MISS: {Key}", key);
        return value;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _options.DefaultTtl
        }, ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _logger.LogDebug("Cache REMOVE: {Key}", key);
        return _cache.RemoveAsync(key, ct);
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        // Redis: SCAN + DEL pattern. Requires Redis-specific implementation.
        // For now, log and delegate to tag-based if possible.
        _logger.LogWarning("Prefix removal requires Redis SCAN - use tag-based invalidation instead");
        return Task.CompletedTask;
    }

    public Task RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        // Redis: Maintain tag->keys sets, or use key patterns
        _logger.LogWarning("Tag removal requires Redis sets - not implemented");
        return Task.CompletedTask;
    }
}
