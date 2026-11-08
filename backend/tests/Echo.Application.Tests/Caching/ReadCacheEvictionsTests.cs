using Echo.Application.Caching;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging.Abstractions;

namespace Echo.Application.Tests.Caching;

[Trait("Category", "Unit")]
public class ReadCacheEvictionsTests
{
    private sealed class DownStore : IOutputCacheStore
    {
        public CancellationToken Seen;

        public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
        {
            Seen = cancellationToken;
            throw new InvalidOperationException("the store is down");
        }

        public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken) =>
            ValueTask.FromResult<byte[]?>(null);

        public ValueTask SetAsync(
            string key,
            byte[] value,
            string[]? tags,
            TimeSpan validFor,
            CancellationToken cancellationToken
        ) => ValueTask.CompletedTask;
    }

    // A drop runs after a write has committed, so a store that fails must not turn that write into an
    // error, and a read that overlapped the drop must still be kept out of the cache.
    [Fact]
    public async Task ADropThatFails_DoesNotThrow_AndStillCounts()
    {
        var evictions = new ReadCacheEvictions(
            new DownStore(),
            NullLogger<ReadCacheEvictions>.Instance
        );

        await evictions.Drop(Guid.NewGuid());

        Assert.Equal(1, evictions.Drops);
    }

    [Fact]
    public async Task ADrop_IsNeverCancelled_ByTheRequestThatCausedIt()
    {
        var store = new DownStore();
        var evictions = new ReadCacheEvictions(store, NullLogger<ReadCacheEvictions>.Instance);

        await evictions.Drop(ReadCache.Views);

        Assert.False(store.Seen.CanBeCanceled);
    }

    [Fact]
    public async Task EachDrop_IsCounted()
    {
        var evictions = new ReadCacheEvictions(
            new RecordingCacheStore(),
            NullLogger<ReadCacheEvictions>.Instance
        );

        await evictions.Drop(ReadCache.Views);
        await evictions.Drop(Guid.NewGuid());

        Assert.Equal(2, evictions.Drops);
    }

    [Fact]
    public void ASynchronousSave_FailsLoudly_RatherThanSkippingTheDrop()
    {
        var interceptor = new CacheEvictionInterceptor(
            new ReadCacheEvictions(
                new RecordingCacheStore(),
                NullLogger<ReadCacheEvictions>.Instance
            )
        );

        Assert.Throws<NotSupportedException>(() => interceptor.SavingChanges(null!, default));
    }
}
