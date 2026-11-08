using System.Collections.Concurrent;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Caching;

/// <summary>
/// Makes cached answers stale. Every congregation has a version, and the version is part of the key of each of
/// its answers. Invalidating raises the version first, so nothing asks for the old answers again, and then
/// removes them to free the memory.
/// <br/><br/>
/// Removing alone would not be safe: a read that began before a write can finish after the removal and store
/// what it read, which is now old, under the key everyone else reads. With the version in the key, that read
/// stores under the old version's key instead, which nothing asks for again.
/// </summary>
public sealed partial class ReadCacheInvalidator(
    IOutputCacheStore store,
    ILogger<ReadCacheInvalidator> logger
)
{
    private readonly ConcurrentDictionary<Guid, long> _congregations = new();
    private long _views;

    /// <summary>
    /// The part of an answer's key that changes when the answer goes stale. Answers built from a materialized
    /// view also go stale when the views are refreshed, which no write to a congregation announces.
    /// </summary>
    public string Version(Guid congregationId, bool readsViews) =>
        $"{_congregations.GetValueOrDefault(congregationId)}.{(readsViews ? Interlocked.Read(ref _views) : 0)}";

    public ValueTask Invalidate(Guid congregationId)
    {
        _congregations.AddOrUpdate(congregationId, 1, (_, version) => version + 1);
        return Remove(ReadCache.Tag(congregationId));
    }

    public ValueTask InvalidateViews()
    {
        Interlocked.Increment(ref _views);
        return Remove(ReadCache.Views);
    }

    // This runs after a write has committed, so it must not undo that write: it never throws and ignores the
    // request's cancellation (a client that hangs up now must not leave old answers in memory). If the store
    // is down, the answers are already unreachable and expire on their own.
    private async ValueTask Remove(string tag)
    {
        try
        {
            await store.EvictByTagAsync(tag, CancellationToken.None);
        }
        catch (Exception ex)
        {
            RemoveFailed(logger, ex, tag);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Could not remove the cached answers tagged {Tag}; they are no longer served and expire on their own"
    )]
    private static partial void RemoveFailed(ILogger logger, Exception exception, string tag);
}
