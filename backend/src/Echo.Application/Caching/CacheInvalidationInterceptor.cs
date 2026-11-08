using System.Data.Common;
using System.Runtime.CompilerServices;
using Echo.Domain.Congregations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Echo.Application.Caching;

/// <summary>
/// Invalidates the cached answers of every congregation a save changed, once the save has committed. It waits
/// for the commit because invalidating earlier would let a read in between store the old data under the new
/// version. A bulk statement such as ExecuteUpdate never reaches here; the code that runs one invalidates itself.
/// </summary>
public sealed class CacheInvalidationInterceptor(ReadCacheInvalidator cache)
    : ISaveChangesInterceptor,
        IDbTransactionInterceptor
{
    // Contexts are pooled and reused, so the congregations waiting for the commit are kept per context.
    private readonly ConditionalWeakTable<DbContext, HashSet<Guid>> _pending = [];

    // A synchronous save would skip the invalidation without a sound, so it fails loudly instead.
    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    ) =>
        throw new NotSupportedException(
            "Save with SaveChangesAsync: a synchronous save would not invalidate the cached answers."
        );

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default
    )
    {
        if (eventData.Context is { } db)
            Pending(db).UnionWith(CongregationsOf(db.ChangeTracker.Entries()));
        return ValueTask.FromResult(result);
    }

    public async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default
    )
    {
        // Inside a transaction of our own the write is not visible yet; its commit invalidates.
        if (eventData.Context is { Database.CurrentTransaction: null } db)
            await InvalidatePending(db);
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
    ) => eventData.Context is { } db ? InvalidatePending(db) : Task.CompletedTask;

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
    /// The congregations a save changes: the one each changed row belongs to. Login and reset tokens belong
    /// to none and no read shows them. A new kind of row that belongs to none must be named in the test that
    /// guards this, or changes to it would never invalidate anything.
    /// </summary>
    public static IEnumerable<Guid> CongregationsOf(IEnumerable<EntityEntry> entries) =>
        entries
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(CongregationOf)
            .OfType<Guid>()
            .Distinct();

    private static Guid? CongregationOf(EntityEntry entry) =>
        entry.Entity is Congregation congregation
            ? congregation.Id
            : entry.Metadata.FindProperty("CongregationId") is null
                ? null
                : entry.Property("CongregationId").CurrentValue as Guid?;

    private HashSet<Guid> Pending(DbContext db) => _pending.GetOrCreateValue(db);

    private void Forget(DbContext? db)
    {
        if (db is not null)
            Pending(db).Clear();
    }

    private async Task InvalidatePending(DbContext db)
    {
        var congregations = Pending(db).ToArray();
        Pending(db).Clear();
        foreach (var congregation in congregations)
            await cache.Invalidate(congregation);
    }
}
