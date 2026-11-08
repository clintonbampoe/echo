using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Echo.Application.Caching;

/// <summary>
/// Every read endpoint that opts in with <c>.CacheOutput(ReadCache.Policy)</c> keeps its answer for
/// <c>Cache:DefaultTtl</c> (10 minutes), per person and per request. A write to anything in a congregation
/// drops that congregation's answers at once (<see cref="CacheEvictionInterceptor"/>), so the time is only
/// the longest an answer lives when nothing was written.
/// </summary>
public static class ReadCache
{
    public const string Policy = "reads";

    /// <summary>
    /// <see cref="Policy"/> for the groups that read a materialized view (the attendance, session, event and
    /// project lists and summaries). The view changes when it is refreshed, not when something is written, so
    /// the refresher drops <see cref="Views"/>. A group that starts reading a view must use this policy.
    /// </summary>
    public const string ViewPolicy = "reads-from-views";

    public const string Views = "views";

    public static string Tag(Guid congregationId) => $"congregation:{congregationId}";

    // shortcut: the store is in this process; with more than one API instance, move to the Redis store.
    public static IServiceCollection AddReadCache(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var ttl = configuration.GetValue("Cache:DefaultTtl", TimeSpan.FromMinutes(10));
        return services.AddOutputCache(o =>
        {
            o.AddPolicy(Policy, new ReadPolicy(ttl));
            o.AddPolicy(ViewPolicy, new ReadPolicy(ttl, Views));
        });
    }
}

// Replaces the built-in rules rather than adding to them, because those refuse any request that is
// signed in. Here a request is only cached when it is a GET by a signed-in person, so anything we
// cannot pin to one person and one congregation is simply not cached.
internal sealed class ReadPolicy(TimeSpan ttl, string? extraTag = null) : IOutputCachePolicy
{
    private static readonly object Started = new();

    private static ReadCacheEvictions Evictions(HttpContext http) =>
        http.RequestServices.GetRequiredService<ReadCacheEvictions>();

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken ct)
    {
        var http = context.HttpContext;
        if (!HttpMethods.IsGet(http.Request.Method) || http.User.Identity?.IsAuthenticated != true)
            return ValueTask.CompletedTask;

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = ttl;
        context.CacheVaryByRules.QueryKeys = "*";
        // The CORS headers are kept with the answer, so each origin needs its own.
        context.CacheVaryByRules.HeaderNames = HeaderNames.Origin;
        context.CacheVaryByRules.VaryByValues["user"] = http.User.GetUserId().ToString();
        context.Tags.Add(ReadCache.Tag(http.User.GetCongregationId()));
        if (extraTag is not null)
            context.Tags.Add(extraTag);
        http.Items[Started] = Evictions(http).Drops;
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken ct)
    {
        Count(context, cached: true);
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken ct)
    {
        var http = context.HttpContext;
        // Something was dropped while this was being worked out, so it may hold what was there before.
        var dropped = http.Items[Started] is long started && started != Evictions(http).Drops;
        if (
            dropped
            || http.Response.StatusCode != StatusCodes.Status200OK
            || http.Response.Headers.ContainsKey(HeaderNames.SetCookie)
        )
            context.AllowCacheStorage = false;

        Count(context, cached: false);
        return ValueTask.CompletedTask;
    }

    private static void Count(OutputCacheContext context, bool cached)
    {
        var http = context.HttpContext;
        var route = (http.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unknown";
        http.RequestServices.GetRequiredService<ApplicationInstrumentation>()
            .CacheRequests.Add(
                1,
                new KeyValuePair<string, object?>("route", route),
                new KeyValuePair<string, object?>("cached", cached)
            );
    }
}
