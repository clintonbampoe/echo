using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Echo.Application.Caching;

/// <summary>
/// What gets cached: a 200 answer to a GET by a signed-in person. Anything that cannot be pinned to one
/// person in one congregation is not cached. It replaces the built-in rules rather than adding to them,
/// because those refuse every request that is signed in.
/// </summary>
internal sealed class ReadPolicy(TimeSpan ttl, bool readsViews = false) : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken ct)
    {
        var http = context.HttpContext;
        if (!HttpMethods.IsGet(http.Request.Method) || http.User.Identity?.IsAuthenticated != true)
            return ValueTask.CompletedTask;

        var congregationId = http.User.GetCongregationId();
        var invalidator = http.RequestServices.GetRequiredService<ReadCacheInvalidator>();

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = ttl;

        // What makes two requests share an answer: the same path and query, from the same person and origin
        // (the CORS headers are kept with the answer), while nothing has made it stale.
        context.CacheVaryByRules.QueryKeys = "*";
        context.CacheVaryByRules.HeaderNames = HeaderNames.Origin;
        context.CacheVaryByRules.VaryByValues["user"] = http.User.GetUserId().ToString();
        context.CacheVaryByRules.VaryByValues["version"] = invalidator.Version(
            congregationId,
            readsViews
        );

        context.Tags.Add(ReadCache.Tag(congregationId));
        if (readsViews)
            context.Tags.Add(ReadCache.Views);
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken ct)
    {
        Count(context, cached: true);
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken ct)
    {
        var response = context.HttpContext.Response;
        if (
            response.StatusCode != StatusCodes.Status200OK
            || response.Headers.ContainsKey(HeaderNames.SetCookie)
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
