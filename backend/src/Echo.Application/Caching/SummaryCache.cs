using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Echo.Shared.Caching;
using Echo.Shared.HttpResults;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Caching;

/// <summary>
/// Serves every summary endpoint: the answer for a congregation and a request is worked out once and
/// kept for the default cache time, and each call is logged and traced as computed or cached.
/// </summary>
public sealed class SummaryCache(
    ICacheService cache,
    ApplicationInstrumentation instrumentation,
    ILogger<SummaryCache> logger
)
{
    /// <param name="name">Which summary this is, for the cache key, the logs and the trace.</param>
    /// <param name="congregationId">Whose summary. A congregation never sees another's.</param>
    /// <param name="request">
    /// Everything the answer depends on besides the congregation, normally the filters as they were
    /// asked for. Two requests with equal values share one answer.
    /// </param>
    /// <param name="compute">Works the summary out; only called when it is not cached.</param>
    public async Task<IOperationResult> Serve<T>(
        string name,
        Guid congregationId,
        object request,
        Func<CancellationToken, Task<T>> compute,
        CancellationToken ct
    )
        where T : class
    {
        using var activity = instrumentation.ActivitySource.StartActivity($"summary.{name}");
        activity?.SetTag("summary.name", name);

        var computed = false;
        var key = CacheKey.Build($"summary:{name}", congregationId, Fingerprint(request));
        var value = await cache.GetOrCreateAsync(
            key,
            async token =>
            {
                computed = true;
                SummaryLog.Computed(logger, name, congregationId);
                return await compute(token);
            },
            ttl: null,
            ct
        );

        activity?.SetTag("summary.cached", !computed);
        SummaryLog.Served(logger, name, congregationId, cached: !computed);
        return new SuccessResult<T>(value!);
    }

    // A short, fixed-length stand-in for the request, so a long list of filters makes a short key.
    private static string Fingerprint(object request) =>
        Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))))
            .ToLowerInvariant()[..16];
}
