// backend/src/Echo.Shared/Caching/CachePolicy.cs
namespace Echo.Shared.Caching;

/// <summary>
/// Defines caching behavior for a specific data type.
/// </summary>
public readonly record struct CachePolicy
{
    /// <summary>Time-to-live for cached entries.</summary>
    public TimeSpan Ttl { get; init; }

    /// <summary>Key prefix for this data type (e.g., "dashboard:attendance:summary:chart").</summary>
    public string KeyPrefix { get; init; }

    /// <summary>Tags for invalidation (e.g., ["attendance", "dashboard"]).</summary>
    public IReadOnlyList<string> Tags { get; init; }

    /// <summary>Whether to serialize nulls (avoid cache stampede on missing data).</summary>
    public bool CacheNulls { get; init; }

    public static CachePolicy Default =>
        new()
        {
            Ttl = TimeSpan.FromMinutes(2),
            KeyPrefix = "default",
            Tags = [],
            CacheNulls = false,
        };
}
