using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Transactions;

public class TransactionCategoryRepository(AppDbContext context)
{
    private readonly DbSet<TransactionCategory> _dbSet = context.Set<TransactionCategory>();

    public async Task<List<TransactionCategory>> GetAll(Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(c => c.CongregationId == congregationId)
            .ToListAsync(ct);
    }

    public async Task<TransactionCategory?> GetById(
        Guid congregationId,
        int id,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(c => c.Id == id && c.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<TransactionCategory>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(tc => tc.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<int, int>> GetTransactionCounts(
        Guid congregationId,
        IReadOnlyList<int> categoryIds,
        CancellationToken ct = default
    )
    {
        if (categoryIds.Count == 0)
            return [];

        return await context
            .Set<Transaction>()
            .AsNoTracking()
            .Where(t => t.CongregationId == congregationId)
            .CountPerTransactionCategory(categoryIds)
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
    }

    public void Create(TransactionCategory entity)
    {
        _dbSet.Add(entity);
    }

    public void BatchCreate(IList<TransactionCategory> entities)
    {
        _dbSet.AddRange(entities);
    }
  
    public async Task<bool> IsInUse(Guid congregationId, int id, CancellationToken ct)
    {
        return await context
            .Set<Transaction>()
            .FilterDeleted()
            .AnyAsync(e => e.CongregationId == congregationId && e.CategoryId == id, ct);
    }

    // Moves every live transaction of the source category to the target, and optionally soft-deletes the
    // source, in one database transaction. Returns how many rows moved.
    public async Task<int> Reassign(
        Guid congregationId,
        TransactionCategory source,
        int targetId,
        bool deleteSource,
        CancellationToken ct
    )
    {
        await using var tx = await context.Database.BeginTransactionAsync(ct);

        var moved = await context
            .Set<Transaction>()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId && e.CategoryId == source.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.CategoryId, targetId), ct);

        if (deleteSource)
        {
            SoftDelete(source);
            await context.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);
        return moved;
    }

    public void SoftDelete(TransactionCategory entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class TransactionCategoryQueryExtensions
{
    internal static IQueryable<CategoryCount> CountPerTransactionCategory(
        this IQueryable<Transaction> query,
        IReadOnlyList<int> categoryIds
    ) =>
        query
            .FilterDeleted()
            .Where(t => categoryIds.Contains(t.CategoryId))
            .GroupBy(t => t.CategoryId)
            .Select(g => new CategoryCount(g.Key, g.Count()));

}
