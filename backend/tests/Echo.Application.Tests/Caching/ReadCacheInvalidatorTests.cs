using Echo.Application.Caching;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging.Abstractions;

namespace Echo.Application.Tests.Caching;

[Trait("Category", "Unit")]
public class ReadCacheInvalidatorTests
{
    private static readonly Guid Alpha = Guid.NewGuid();
    private static readonly Guid Beta = Guid.NewGuid();

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

    private static ReadCacheInvalidator Invalidator(IOutputCacheStore store) =>
        new(store, NullLogger<ReadCacheInvalidator>.Instance);

    [Fact]
    public async Task InvalidatingACongregation_ChangesItsVersion_AndNoOthers()
    {
        var invalidator = Invalidator(new RecordingCacheStore());
        var other = invalidator.Version(Beta, readsViews: false);
        var before = invalidator.Version(Alpha, readsViews: false);

        await invalidator.Invalidate(Alpha);

        Assert.NotEqual(before, invalidator.Version(Alpha, readsViews: false));
        Assert.Equal(other, invalidator.Version(Beta, readsViews: false));
    }

    [Fact]
    public async Task InvalidatingTheViews_ChangesTheVersionOfReadsFromViewsOnly()
    {
        var invalidator = Invalidator(new RecordingCacheStore());
        var plain = invalidator.Version(Alpha, readsViews: false);
        var fromViews = invalidator.Version(Alpha, readsViews: true);

        await invalidator.InvalidateViews();

        Assert.Equal(plain, invalidator.Version(Alpha, readsViews: false));
        Assert.NotEqual(fromViews, invalidator.Version(Alpha, readsViews: true));
    }

    [Fact]
    public async Task TheStoredAnswers_AreRemovedToFreeTheMemory()
    {
        var store = new RecordingCacheStore();
        var invalidator = Invalidator(store);

        await invalidator.Invalidate(Alpha);
        await invalidator.InvalidateViews();

        Assert.Equal([ReadCache.Tag(Alpha), ReadCache.Views], store.Evicted);
    }

    // It runs after a write has committed, so a store that fails must not turn that write into an error, and
    // the old answers must be unreachable even though they could not be removed.
    [Fact]
    public async Task AStoreThatFails_DoesNotThrow_AndTheOldAnswersAreStillUnreachable()
    {
        var invalidator = Invalidator(new DownStore());
        var before = invalidator.Version(Alpha, readsViews: false);

        await invalidator.Invalidate(Alpha);

        Assert.NotEqual(before, invalidator.Version(Alpha, readsViews: false));
    }

    [Fact]
    public async Task RemovingAnswers_IsNeverCancelled_ByTheRequestThatCausedIt()
    {
        var store = new DownStore();

        await Invalidator(store).InvalidateViews();

        Assert.False(store.Seen.CanBeCanceled);
    }
}
