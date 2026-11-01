using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Echo.Shared.Caching;
using Echo.Shared.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Echo.Application.Caching;

/// <summary>
/// Serves every summary endpoint: the answer for a congregation and a request is worked out once and
/// kept for the default cache time (5 minutes), and each call is logged and traced as computed or cached.
/// <br/><br/>
/// An answer is refreshed before it runs out. Once it is three quarters through its life, the next
/// request still gets it straight away and starts one refresh in the background, so people who keep
/// using the app never wait for a summary to be rebuilt. An answer nobody asks for is left to expire.
/// </summary>
public sealed class SummaryCache(
    ICacheService cache,
    ApplicationInstrumentation instrumentation,
    IServiceScopeFactory scopes,
    IOptions<CacheOptions> options,
    TimeProvider time,
    ILogger<SummaryCache> logger
)
{
    // The share of an answer's life after which a request also refreshes it.
    private const double RefreshAfter = 0.75;

    // The keys being refreshed right now, across all requests, so a busy page starts one refresh, not twenty.
    private static readonly ConcurrentDictionary<string, byte> _refreshing = new();

    private sealed record Entry<T>(T Value, DateTimeOffset RefreshAt);

    /// <param name="owner">
    /// The service that is asking, which works the summary out if it is not cached. A refresh happens
    /// after the request is over, so it cannot use this one; it asks the container for another.
    /// </param>
    /// <param name="name">Which summary this is, for the cache key, the logs and the trace.</param>
    /// <param name="congregationId">Whose summary. A congregation never sees another's.</param>
    /// <param name="request">
    /// Everything the answer depends on besides the congregation, normally the filters as they were
    /// asked for. Two requests with equal values share one answer.
    /// </param>
    /// <param name="compute">Works the summary out, on the service it is given.</param>
    public async Task<IOperationResult> Serve<TService, T>(
        TService owner,
        string name,
        Guid congregationId,
        object request,
        Func<TService, CancellationToken, Task<T>> compute,
        CancellationToken ct
    )
        where TService : notnull
        where T : class
    {
        using var activity = instrumentation.ActivitySource.StartActivity($"summary.{name}");
        activity?.SetTag("summary.name", name);

        var key = CacheKey.Build($"summary:{name}", congregationId, Fingerprint(request));
        var entry = await cache.GetAsync<Entry<T>>(key, ct);
        var cached = entry is not null;

        if (entry is null)
        {
            SummaryLog.Computed(logger, name, congregationId);
            entry = await Store(key, await compute(owner, ct), ct);
        }
        else if (time.GetUtcNow() >= entry.RefreshAt)
        {
            Refresh(key, name, congregationId, compute);
        }

        activity?.SetTag("summary.cached", cached);
        instrumentation.SummaryRequests.Add(
            1,
            new KeyValuePair<string, object?>("summary", name),
            new KeyValuePair<string, object?>("cached", cached)
        );
        SummaryLog.Served(logger, name, congregationId, cached);
        return new SuccessResult<T>(entry.Value);
    }

    private async Task<Entry<T>> Store<T>(string key, T value, CancellationToken ct)
        where T : class
    {
        var ttl = options.Value.DefaultTtl;
        var entry = new Entry<T>(value, time.GetUtcNow().Add(ttl * RefreshAfter));
        await cache.SetAsync(key, entry, ttl, ct);
        return entry;
    }

    private void Refresh<TService, T>(
        string key,
        string name,
        Guid congregationId,
        Func<TService, CancellationToken, Task<T>> compute
    )
        where TService : notnull
        where T : class
    {
        if (!_refreshing.TryAdd(key, 0))
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = scopes.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                await Store(
                    key,
                    await compute(service, CancellationToken.None),
                    CancellationToken.None
                );
                SummaryLog.Refreshed(logger, name, congregationId);
            }
            catch (Exception ex)
            {
                SummaryLog.RefreshFailed(logger, ex, name, congregationId);
            }
            finally
            {
                _refreshing.TryRemove(key, out _);
            }
        });
    }

    // A short, fixed-length stand-in for the request, so a long list of filters makes a short key.
    private static string Fingerprint(object request) =>
        Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))))
            .ToLowerInvariant()[..16];
}
