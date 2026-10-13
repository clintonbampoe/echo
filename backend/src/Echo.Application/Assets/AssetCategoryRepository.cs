using Echo.Data;
using Echo.Domain.Assets;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Assets;

public class AssetCategoryRepository(AppDbContext context)
{
    private readonly DbSet<AssetCategory> _dbSet = context.Set<AssetCategory>();

    public async Task<List<AssetCategory>> GetAll(Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(c => c.CongregationId == congregationId)
            .ToListAsync(ct);
    }

    public async Task<AssetCategory?> GetById(Guid congregationId, int id, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(c => c.Id == id && c.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<AssetCategory>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<int, int>> GetAssetCounts(
        Guid congregationId,
        IReadOnlyList<int> categoryIds,
        CancellationToken ct = default
    )
    {
        if (categoryIds.Count == 0)
            return [];

        return await context
            .Set<Asset>()
            .AsNoTracking()
            .Where(a => a.CongregationId == congregationId)
            .CountPerAssetCategory(categoryIds)
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
    }

    public void Create(AssetCategory entity)
    {
        _dbSet.Add(entity);
    }

    public void BatchCreate(IList<AssetCategory> entities)
    {
        _dbSet.AddRange(entities);
    }
  
    public async Task<bool> IsInUse(Guid congregationId, int id, CancellationToken ct)
    {
        return await context
            .Set<Asset>()
            .FilterDeleted()
            .AnyAsync(e => e.CongregationId == congregationId && e.CategoryId == id, ct);
    }

    // Moves every live asset of the source category to the target, and optionally soft-deletes the
    // source, in one database transaction. Returns how many rows moved.
    public async Task<int> Reassign(
        Guid congregationId,
        AssetCategory source,
        int targetId,
        bool deleteSource,
        CancellationToken ct
    )
    {
        await using var tx = await context.Database.BeginTransactionAsync(ct);

        var moved = await context
            .Set<Asset>()
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

    public void SoftDelete(AssetCategory entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class AssetCategoryQueryExtensions
{
    internal static IQueryable<CategoryCount> CountPerAssetCategory(
        this IQueryable<Asset> query,
        IReadOnlyList<int> categoryIds
    ) =>
        query
            .FilterDeleted()
            .Where(a => categoryIds.Contains(a.CategoryId))
            .GroupBy(a => a.CategoryId)
            .Select(g => new CategoryCount(g.Key, g.Count()));

}
