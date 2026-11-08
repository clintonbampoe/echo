using System.Collections.Concurrent;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Caching;

/// <summary>
/// The one way cached answers are dropped.
/// <br/><br/>
/// Dropping alone cannot be made safe: a read that began before a write can finish after the drop and store
/// what it read, which is now old, under the key everyone else reads. So every congregation has a version,
/// and the version is part of the key. A drop raises the version first and then removes what is stored. A read
/// that began earlier stores under the old version's key, which nothing asks for again, and every read after
/// the drop asks for the new one and works its answer out from the data as it is now. The removal only frees
/// memory sooner; being right does not depend on it.
/// </summary>
public sealed partial class ReadCacheEvictions(
    IOutputCacheStore store,
    ILogger<ReadCacheEvictions> logger
)
{
    private readonly ConcurrentDictionary<Guid, long> _congregations = new();
    private long _views;

    /// <summary>
    /// What goes in the key of an answer for this congregation. Answers that read a materialized view also
    /// change when the views are refreshed, which no write to a congregation announces.
    /// </summary>
    public string Version(Guid congregationId, bool readsViews) =>
        $"{_congregations.GetValueOrDefault(congregationId)}.{(readsViews ? Interlocked.Read(ref _views) : 0)}";

    public ValueTask Drop(Guid congregationId)
    {
        _congregations.AddOrUpdate(congregationId, 1, (_, version) => version + 1);
        return Remove(ReadCache.Tag(congregationId));
    }

    public ValueTask DropViews()
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
