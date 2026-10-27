namespace Echo.Shared.Caching;

/// <summary>
/// Configuration options for the caching abstraction.
/// </summary>
public sealed class CacheOptions
{
    /// <summary>
    /// Configuration section name: "Cache".
    /// </summary>
    public const string Section = "Cache";

    /// <summary>
    /// Default time-to-live for cache entries when no explicit TTL is provided.
    /// Default: 2 minutes.
    /// </summary>
    public TimeSpan DefaultTtl { get; init; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Whether to cache null values to prevent cache stampede on missing data.
    /// When true, a null result from the factory will be cached for the TTL duration.
    /// Default: false.
    /// </summary>
    public bool CacheNulls { get; init; } = false;
}
