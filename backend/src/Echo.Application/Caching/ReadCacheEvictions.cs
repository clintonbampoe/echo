using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Caching;

/// <summary>
/// The one way cached answers are dropped. It also counts the drops: a read that was being worked out while
/// one happened may have read the data from before it, so it is not kept (see <see cref="ReadPolicy"/>).
/// </summary>
public sealed partial class ReadCacheEvictions(
    IOutputCacheStore store,
    ILogger<ReadCacheEvictions> logger
)
{
    private long _drops;

    public long Drops => Interlocked.Read(ref _drops);

    // This runs after a write has committed, so it must not undo that write: it never throws and ignores the
    // request's cancellation (a client that hangs up now must not leave old answers behind). If the store is
    // down, the answers expire on their own.
    public async ValueTask Drop(string tag)
    {
        Interlocked.Increment(ref _drops);
        try
        {
            await store.EvictByTagAsync(tag, CancellationToken.None);
        }
        catch (Exception ex)
        {
            DropFailed(logger, ex, tag);
        }
    }

    public ValueTask Drop(Guid congregationId) => Drop(ReadCache.Tag(congregationId));

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Could not drop the cached answers tagged {Tag}; they expire on their own"
    )]
    private static partial void DropFailed(ILogger logger, Exception exception, string tag);
}
