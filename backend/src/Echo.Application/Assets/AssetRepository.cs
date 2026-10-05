using Echo.Data;
using Echo.Domain.Assets;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Assets;

public class AssetRepository(AppDbContext context, TimeProvider timeProvider)
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
            .Filter(filters, timeProvider)
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

    public void Create(Asset entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Asset entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task<int> Count(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<decimal> SumCurrentValue(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .SumAsync(a => a.CurrentValue, ct);

    public Task<decimal> SumPurchaseCost(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .SumAsync(a => a.PurchaseCost, ct);

    public Task<int> CountByStatus(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .CountAsync(ct);
}

internal static class AssetQueryExtensions
{
    internal static IQueryable<Asset> Filter(
        this IQueryable<Asset> query,
        AssetFilters filters,
        TimeProvider timeProvider
    )
    {
        if (filters.Name is not null)
            query = query.Where(a => EF.Functions.ILike(a.Name, $"%{filters.Name}%"));

        if (filters.CategoryId is not null)
            query = query.Where(a => a.CategoryId == filters.CategoryId);

        if (filters.Status is not null)
            query = query.Where(a => a.Status == filters.Status);

        var from = filters.From ?? DateUtils.GetFirstDayOfYear(timeProvider);
        var to = filters.To ?? DateUtils.GetLastDayOfYear(timeProvider);

        query = query.Where(a => a.PurchaseDate >= from && a.PurchaseDate <= to);

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
