using Echo.Application.Assets;
using Echo.Application.Caching;
using Echo.Data;
using Echo.Domain.Assets;
using Echo.Domain.Congregations;
using Echo.IntegrationTests.Support;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Echo.IntegrationTests.Caching;

// The cache must be invalidated after a write is committed, not before it and not when it never happens.
[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class CacheInvalidationTests(PostgresFixture db)
{
    private sealed class RecordingStore : IOutputCacheStore
    {
        public List<string> Evicted { get; } = [];

        public ValueTask EvictByTagAsync(string tag, CancellationToken ct)
        {
            Evicted.Add(tag);
            return ValueTask.CompletedTask;
        }

        public ValueTask<byte[]?> GetAsync(string key, CancellationToken ct) =>
            ValueTask.FromResult<byte[]?>(null);

        public ValueTask SetAsync(
            string key,
            byte[] value,
            string[]? tags,
            TimeSpan validFor,
            CancellationToken ct
        ) => ValueTask.CompletedTask;
    }

    private async Task<(AppDbContext Db, RecordingStore Store)> Start()
    {
        await db.ResetAsync();
        var store = new RecordingStore();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(db.ConnectionString);
        options.ConfigureEchoDatabase();
        options.AddInterceptors(new CacheInvalidationInterceptor(new ReadCacheInvalidator(store, NullLogger<ReadCacheInvalidator>.Instance)));
        return (new AppDbContext(options.Options), store);
    }

    private static Congregation Church(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = $"Grace Chapel {Guid.NewGuid():N}",
            PhoneNumber = "0200000000",
            EmailAddress = "office@echo.test",
            City = "Accra",
            Town = "Osu",
            GpsAddress = "GA-000-0000",
        };

    [Fact]
    public async Task ASavedWrite_InvalidatesItsCongregation()
    {
        var (context, store) = await Start();
        await using var _ = context;
        var church = Church();

        context.Add(church);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal([ReadCache.Tag(church.Id)], store.Evicted);
    }

    [Fact]
    public async Task AWriteInsideATransaction_InvalidatesOnlyOnceTheTransactionCommits()
    {
        var (context, store) = await Start();
        await using var _ = context;
        var church = Church();
        var ct = TestContext.Current.CancellationToken;

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        context.Add(church);
        await context.SaveChangesAsync(ct);
        Assert.Empty(store.Evicted);

        await transaction.CommitAsync(ct);

        Assert.Equal([ReadCache.Tag(church.Id)], store.Evicted);
    }

    [Fact]
    public async Task AWriteThatIsRolledBack_InvalidatesNothing()
    {
        var (context, store) = await Start();
        await using var _ = context;
        var ct = TestContext.Current.CancellationToken;

        await using (var transaction = await context.Database.BeginTransactionAsync(ct))
        {
            context.Add(Church());
            await context.SaveChangesAsync(ct);
            await transaction.RollbackAsync(ct);
        }

        Assert.Empty(store.Evicted);
    }

    [Fact]
    public async Task AWriteThatFails_InvalidatesNothing_AndItsCongregationsDoNotLeakIntoTheNextSave()
    {
        var (context, store) = await Start();
        await using var _ = context;
        var ct = TestContext.Current.CancellationToken;
        var first = Church();
        context.Add(first);
        await context.SaveChangesAsync(ct);
        store.Evicted.Clear();

        // Same id as one already saved, so the database refuses it.
        var clash = Church(first.Id);
        context.ChangeTracker.Clear();
        context.Add(clash);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(ct));
        Assert.Empty(store.Evicted);

        context.ChangeTracker.Clear();
        var second = Church();
        context.Add(second);
        await context.SaveChangesAsync(ct);

        Assert.Equal([ReadCache.Tag(second.Id)], store.Evicted);
    }

    // A bulk update is invisible to the save interceptor, so the repository invalidates the cache itself,
    // also when the source category is kept and nothing else is saved.
    [Fact]
    public async Task MovingAssetsToAnotherCategory_InvalidatesTheCongregation_EvenWhenNothingIsDeleted()
    {
        var (context, store) = await Start();
        await using var _ = context;
        var ct = TestContext.Current.CancellationToken;
        var church = Church();
        var from = new AssetCategory { CongregationId = church.Id, Name = "Chairs" };
        var to = new AssetCategory { CongregationId = church.Id, Name = "Seating" };
        context.AddRange(church, from, to);
        await context.SaveChangesAsync(ct);
        store.Evicted.Clear();

        var repository = new AssetCategoryRepository(context, new ReadCacheInvalidator(store, NullLogger<ReadCacheInvalidator>.Instance));
        await repository.Reassign(church.Id, from, to.Id, deleteSource: false, ct);

        Assert.Equal([ReadCache.Tag(church.Id)], store.Evicted);
    }
}
