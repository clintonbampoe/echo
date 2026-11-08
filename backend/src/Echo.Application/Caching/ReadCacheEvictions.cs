using Microsoft.AspNetCore.OutputCaching;

namespace Echo.Application.Caching;

/// <summary>
/// The one way cached answers are dropped. It also counts the drops: a read that was being worked out while
/// one happened may have read the data from before it, so it is not kept (see <see cref="ReadPolicy"/>).
/// </summary>
public sealed class ReadCacheEvictions(IOutputCacheStore store)
{
    private long _drops;

    public long Drops => Interlocked.Read(ref _drops);

    public ValueTask Drop(string tag, CancellationToken ct = default)
    {
        Interlocked.Increment(ref _drops);
        return store.EvictByTagAsync(tag, ct);
    }

    public ValueTask Drop(Guid congregationId, CancellationToken ct = default) =>
        Drop(ReadCache.Tag(congregationId), ct);
}
