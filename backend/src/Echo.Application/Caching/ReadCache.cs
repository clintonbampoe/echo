using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Caching;

/// <summary>
/// A read endpoint opts in with <c>.CacheOutput(ReadCache.Policy)</c> and its answer is kept for
/// <c>Cache:DefaultTtl</c> (10 minutes), per person and per request. A write to anything in a congregation
/// makes that congregation's answers stale at once (<see cref="CacheInvalidationInterceptor"/>), so the time
/// is only the longest an answer lives when nothing was written. See <see cref="ReadPolicy"/> for what is cached.
/// </summary>
public static class ReadCache
{
    public const string Policy = "reads";

    /// <summary>
    /// <see cref="Policy"/> for the groups that read a materialized view (the attendance, session, event and
    /// project lists and summaries). A view changes when it is refreshed, not when something is written, so
    /// the refresher invalidates these separately. A group that starts reading a view must use this policy.
    /// </summary>
    public const string ViewPolicy = "reads-from-views";

    /// <summary>The tag on the answers that read a view, to remove them from memory.</summary>
    public const string Views = "views";

    /// <summary>The tag on every answer, to remove a congregation's answers from memory.</summary>
    public static string Tag(Guid congregationId) => $"congregation:{congregationId}";

    // shortcut: the store and the versions are in this process; with more than one API instance, share both.
    public static IServiceCollection AddReadCache(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var ttl = configuration.GetValue("Cache:DefaultTtl", TimeSpan.FromMinutes(10));
        return services.AddOutputCache(o =>
        {
            o.AddPolicy(Policy, new ReadPolicy(ttl));
            o.AddPolicy(ViewPolicy, new ReadPolicy(ttl, readsViews: true));
        });
    }
}
