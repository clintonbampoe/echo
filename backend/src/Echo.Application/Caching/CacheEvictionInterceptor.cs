using System.Data.Common;
using System.Runtime.CompilerServices;
using Echo.Domain.Congregations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Echo.Application.Caching;

/// <summary>
/// Drops a congregation's cached answers once a write to it has committed, so the next read sees it.
/// It waits for the commit: dropping earlier would let a read in between cache the old data again.
/// A bulk statement such as ExecuteUpdate never reaches here; the code that runs one drops the cache itself.
/// </summary>
public sealed class CacheEvictionInterceptor(ReadCacheEvictions cache)
    : ISaveChangesInterceptor,
        IDbTransactionInterceptor
{
    // Contexts are pooled and reused, so what waits to be dropped is kept per context, not per request.
    private readonly ConditionalWeakTable<DbContext, HashSet<string>> _pending = [];

    // A synchronous save would skip the drop without a sound, so it fails loudly instead.
    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    ) =>
        throw new NotSupportedException(
            "Save with SaveChangesAsync: a synchronous save would not drop the cached answers."
        );

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default
    )
    {
        if (eventData.Context is { } db)
            Pending(db).UnionWith(TagsOf(db.ChangeTracker.Entries()));
        return ValueTask.FromResult(result);
    }

    public async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default
    )
    {
        // Inside our own transaction the write is not visible yet; its commit drops the cache.
        if (eventData.Context is { Database.CurrentTransaction: null } db)
            await Evict(db);
        return result;
    }

    public Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken ct = default
    )
    {
        Forget(eventData.Context);
        return Task.CompletedTask;
    }

    public Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken ct = default
    ) => eventData.Context is { } db ? Evict(db) : Task.CompletedTask;

    public Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken ct = default
    )
    {
        Forget(eventData.Context);
        return Task.CompletedTask;
    }

    /// <summary>
    /// The cache tags of what a save changes: the congregation each changed row belongs to. The login and
    /// reset tokens belong to none and no read shows them. A new kind of row that belongs to none must be
    /// named here, which a test enforces, or its changes would never drop a cached answer.
    /// </summary>
    public static IEnumerable<string> TagsOf(IEnumerable<EntityEntry> entries) =>
        entries
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(CongregationOf)
            .OfType<Guid>()
            .Distinct()
            .Select(ReadCache.Tag);

    private static Guid? CongregationOf(EntityEntry entry) =>
        entry.Entity is Congregation congregation
            ? congregation.Id
            : entry.Metadata.FindProperty("CongregationId") is null
                ? null
                : entry.Property("CongregationId").CurrentValue as Guid?;

    private HashSet<string> Pending(DbContext db) => _pending.GetOrCreateValue(db);

    private void Forget(DbContext? db)
    {
        if (db is not null)
            Pending(db).Clear();
    }

    private async Task Evict(DbContext db)
    {
        var tags = Pending(db).ToArray();
        Pending(db).Clear();
        foreach (var tag in tags)
            await cache.Drop(tag);
    }
}
