using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Echo.Shared.Caching;

/// <summary>
/// In-memory implementation of <see cref="ICacheService"/> using <see cref="IMemoryCache"/>.
/// Suitable for single-instance deployments. For distributed scenarios, use a Redis-backed implementation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InMemoryCacheService"/> class.
/// </remarks>
/// <param name="cache">The underlying memory cache.</param>
/// <param name="logger">Logger for cache operations.</param>
/// <param name="options">Cache configuration options.</param>
public sealed partial class InMemoryCacheService(
    IMemoryCache cache,
    ILogger<InMemoryCacheService> logger,
    IOptions<CacheOptions> options
) : ICacheService
{
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<InMemoryCacheService> _logger = logger;
    private readonly CacheOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default
    )
        where T : class
    {
        if (_cache.TryGetValue(key, out T? cached))
        {
            Log.CacheHit(_logger, key);
            return cached;
        }

        Log.CacheMiss(_logger, key);
        var value = await factory(ct).ConfigureAwait(false);

        if (value is not null || _options.CacheNulls)
        {
            var entry = _cache.CreateEntry(key);
            entry.Value = value;
            entry.AbsoluteExpirationRelativeToNow = ttl ?? _options.DefaultTtl;
            entry.Dispose(); // Commits the entry
        }

        return value;
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        where T : class
    {
        _cache.TryGetValue(key, out T? value);

        if (value is not null)
            Log.CacheHit(_logger, key);
        else
            Log.CacheMiss(_logger, key);

        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? ttl = null,
        CancellationToken ct = default
    )
        where T : class
    {
        var entry = _cache.CreateEntry(key);
        entry.Value = value;
        entry.AbsoluteExpirationRelativeToNow = ttl ?? _options.DefaultTtl;
        entry.Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        Log.CacheRemove(_logger, key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        Log.CachePrefixRemovalUnsupported(_logger, prefix);
        // IMemoryCache has no built-in prefix removal.
        // If needed, maintain a ConcurrentDictionary<prefix, HashSet<key>> externally.
        return Task.CompletedTask;
    }
}

public static partial class Log
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Debug, Message = "Cache HIT: {Key}")]
    public static partial void CacheHit(ILogger logger, string key);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "Cache MISS: {Key}")]
    public static partial void CacheMiss(ILogger logger, string key);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug, Message = "Cache REMOVE: {Key}")]
    public static partial void CacheRemove(ILogger logger, string key);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Prefix removal not supported in-memory; use explicit key removal. Prefix: {Prefix}"
    )]
    public static partial void CachePrefixRemovalUnsupported(ILogger logger, string prefix);
}
