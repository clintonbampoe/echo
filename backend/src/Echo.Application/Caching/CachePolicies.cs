namespace Echo.Application.Caching;

/// <summary>
/// Predefined cache policies for application-specific data.
/// Policies define TTL, key prefix, and null-caching behavior per data type.
/// Keep policies close to their consuming domain for maintainability.
/// </summary>
public static class CachePolicies
{
    /// <summary>
    /// Session labels reference data (rarely changes).
    /// </summary>
    public static readonly CachePolicy SessionLabels = new()
    {
        Ttl = TimeSpan.FromMinutes(2),
        KeyPrefix = "attendance:labels",
        CacheNulls = true,
    };

    /// <summary>
    /// Asset categories reference data (rarely changes).
    /// </summary>
    public static readonly CachePolicy AssetCategories = new()
    {
        Ttl = TimeSpan.FromMinutes(2),
        KeyPrefix = "assets:categories",
        CacheNulls = true,
    };
}

/// <summary>
/// Defines caching behavior for a specific data type.
/// </summary>
/// <param name="Ttl">Time-to-live for cached entries.</param>
/// <param name="KeyPrefix">Logical key prefix (e.g., "dashboard:attendance:summary:chart"). 
/// Combined with congregation ID and qualifiers.</param>
/// <param name="CacheNulls">Whether to cache null results to prevent stampede on missing data.</param>
public readonly record struct CachePolicy
{
    /// <summary>Time-to-live for cached entries.</summary>
    public required TimeSpan Ttl { get; init; }

    /// <summary>
    /// Key prefix for this data type.
    /// Combined with congregation ID and qualifiers via <see cref="CacheKey.Build"/>.
    /// </summary>
    public required string KeyPrefix { get; init; }

    /// <summary>
    /// Whether to cache null values (prevents repeated factory execution for missing data).
    /// </summary>
    public bool CacheNulls { get; init; }
}
