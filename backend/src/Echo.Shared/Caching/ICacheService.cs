namespace Echo.Shared.Caching;

/// <summary>
/// Abstraction over a cache store (in-memory, Redis, etc.).
/// Implementations should be thread-safe and support async operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache, or creates it using the factory if not present.
    /// </summary>
    /// <typeparam name="T">The type of the cached value (must be a reference type).</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Async factory to create the value on cache miss.</param>
    /// <param name="ttl">Optional time-to-live override. Uses <see cref="CacheOptions.DefaultTtl"/> when null.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cached or newly created value, or null if factory returns null and <see cref="CacheOptions.CacheNulls"/> is false.</returns>
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken ct = default
    )
        where T : class;

    /// <summary>
    /// Gets a value from the cache without creating it.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cached value, or null if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        where T : class;

    /// <summary>
    /// Sets a value in the cache with an optional TTL.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ttl">Optional time-to-live override.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
        where T : class;

    /// <summary>
    /// Removes a single cache entry by exact key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Removes all cache entries whose keys start with the given prefix.
    /// Note: Not all implementations support efficient prefix removal (e.g., in-memory cache).
    /// </summary>
    /// <param name="prefix">The key prefix to match.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}
