namespace Echo.Shared.Caching;

/// <summary>
/// Provides consistent, structured cache key generation.
/// Keys follow the pattern: <c>{prefix}:{congregationId}:{qualifiers...}</c> or
/// <c>{prefix}:global:{qualifiers...}</c> for congregation-agnostic data.
/// </summary>
public static class CacheKey
{
    private const char _separator = ':';

    /// <summary>
    /// Builds a cache key scoped to a congregation.
    /// <br/>
    /// Format: <c>{prefix}:{congregationId}:{qualifier1}:{qualifier2}...</c>
    /// </summary>
    /// <param name="prefix">Logical grouping prefix (e.g., "dashboard:attendance:summary:chart").</param>
    /// <param name="congregationId">The congregation identifier for multi-tenancy isolation.</param>
    /// <param name="qualifiers">Additional key parts (e.g., timeframe, filter values).</param>
    /// <returns>A deterministic cache key string.</returns>
    /// <example>
    /// <c>CacheKey.Build("dashboard:attendance:summary:chart", congregationId, 6)</c> →
    /// <c>"dashboard:attendance:summary:chart:123e4567-e89b-12d3-a456-426614174000:6"</c>
    /// </example>
    public static string Build(string prefix, Guid congregationId, params object[] qualifiers)
    {
        var parts = new List<string>(2 + qualifiers.Length) { prefix, congregationId.ToString() };
        parts.AddRange(qualifiers.Select(q => q.ToString()!));
        return string.Join(_separator, parts);
    }

    /// <summary>
    /// Builds a cache key for global (non-congregation-scoped) data.
    /// Format: <c>{prefix}:global:{qualifier1}:{qualifier2}...</c>
    /// </summary>
    /// <param name="prefix">Logical grouping prefix.</param>
    /// <param name="qualifiers">Additional key parts.</param>
    /// <returns>A deterministic cache key string.</returns>
    public static string BuildGlobal(string prefix, params object[] qualifiers)
    {
        var parts = new List<string>(2 + qualifiers.Length) { prefix, "global" };
        parts.AddRange(qualifiers.Select(q => q.ToString()!));
        return string.Join(_separator, parts);
    }
}
