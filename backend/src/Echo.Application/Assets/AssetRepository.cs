using Echo.Data;
using Echo.Domain.Assets;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Domain.Congregations;

namespace Echo.Application.Assets;

public class AssetRepository(AppDbContext context)
{
    private readonly DbSet<Asset> _dbSet = context.Set<Asset>();

    public async Task<List<Asset>> List(
        Guid congregationId,
        AssetFilters filters,
        AssetCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Include(a => a.Category)
            .Filter(filters)
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Asset?> GetById(Guid congregationId, Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(a => a.Id == id && a.CongregationId == congregationId)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Asset>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public async Task<AssetStats> GetSummaryStats(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct = default
    )
    {
        var assets = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters);

        var stats = await context.OneRow(congregationId).SummarizeAssets(assets).FirstOrDefaultAsync(ct);
        return stats ?? new AssetStats(0, 0m, 0m);
    }

    public void Create(Asset entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Asset entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task<int> CountByStatus(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters)
            .CountAsync(ct);
}

public record AssetStats(int Count, decimal CurrentValue, decimal PurchaseCost);

internal static class AssetQueryExtensions
{
    internal static IQueryable<AssetStats> SummarizeAssets(
        this IQueryable<Congregation> oneRow,
        IQueryable<Asset> assets
    ) =>
        oneRow.Select(_ => new AssetStats(
            assets.Count(),
            assets.Sum(a => a.CurrentValue),
            assets.Sum(a => a.PurchaseCost)
        ));

    internal static IQueryable<Asset> Filter(
        this IQueryable<Asset> query,
        AssetFilters filters
    )
    {
        if (filters.Name is not null)
            query = query.Where(a => EF.Functions.ILike(a.Name, $"%{filters.Name}%"));

        if (filters.CategoryId is not null)
            query = query.Where(a => a.CategoryId == filters.CategoryId);

        if (filters.Status is not null)
            query = query.Where(a => a.Status == filters.Status);
        else if (filters.IncludeLiquidated == false)
            query = query.Where(a => a.Status != AssetStatus.Liquidated);

        if (filters.From is not null)
            query = query.Where(a => a.PurchaseDate >= filters.From);

        if (filters.To is not null)
            query = query.Where(a => a.PurchaseDate <= filters.To);

        return query;
    }

    internal static IQueryable<Asset> Paginate(
        this IQueryable<Asset> query,
        AssetCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(a =>
                string.Compare(a.Name, cursor.Name) > 0
                || (a.Name == cursor.Name && a.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
