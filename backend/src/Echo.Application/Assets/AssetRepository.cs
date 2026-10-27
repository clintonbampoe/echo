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
        var today = DateUtils.GetDateToday(timeProvider);
        var stats = await Scope(congregationId, filters).SummarizeAssets(today).FirstOrDefaultAsync(ct);
        return stats ?? new AssetStats(0, 0m, 0m, 0d);
    }

    public async Task<List<AssetCategoryTotalDto>> GetCategoryTotals(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct = default
    ) =>
        await Scope(congregationId, filters)
            .TotalsByCategory(context.Set<AssetCategory>())
            .ToListAsync(ct);

    public async Task<List<AssetMonthRow>> GetAcquiredByMonth(
        Guid congregationId,
        int? categoryId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    ) =>
        await Scope(congregationId, new AssetFilters { CategoryId = categoryId })
            .Where(a => a.PurchaseDate >= from && a.PurchaseDate <= to)
            .AcquiredByMonth()
            .ToListAsync(ct);

    public async Task<List<AssetMonthRow>> GetLostByMonth(
        Guid congregationId,
        int? categoryId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    ) =>
        await Scope(congregationId, new AssetFilters { CategoryId = categoryId })
            .Where(a => a.LiquidatedOn >= from && a.LiquidatedOn <= to)
            .LostByMonth()
            .ToListAsync(ct);

    private IQueryable<Asset> Scope(Guid congregationId, AssetFilters filters) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters);

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

public record AssetStats(int Count, decimal CurrentValue, decimal PurchaseCost, double AverageAgeYears);

public record AssetMonthRow(int Year, int Month, int Count, decimal Cost);

internal static class AssetQueryExtensions
{
    // One pass over the assets for every figure. Age is counted in whole months between the purchase
    // and today, and only over assets that have a purchase date.
    internal static IQueryable<AssetStats> SummarizeAssets(
        this IQueryable<Asset> assets,
        DateOnly today
    ) =>
        assets
            .GroupBy(a => a.CongregationId)
            .Select(group => new AssetStats(
                group.Count(),
                group.Sum(a => a.CurrentValue),
                group.Sum(a => a.PurchaseCost),
                (
                    group.Average(a =>
                        a.PurchaseDate == null
                            ? null
                            : (double?)(
                                (today.Year - a.PurchaseDate.Value.Year) * 12
                                + today.Month
                                - a.PurchaseDate.Value.Month
                            )
                    ) ?? 0d
                ) / 12
            ));

    // Groups first and looks the names up after, so only the few totals go through the join.
    internal static IQueryable<AssetCategoryTotalDto> TotalsByCategory(
        this IQueryable<Asset> assets,
        IQueryable<AssetCategory> categories
    ) =>
        assets
            .GroupBy(a => a.CategoryId)
            .Select(group => new
            {
                CategoryId = group.Key,
                Count = group.Count(),
                Cost = group.Sum(a => a.PurchaseCost),
                Value = group.Sum(a => a.CurrentValue),
            })
            .Join(
                categories,
                total => total.CategoryId,
                category => category.Id,
                (total, category) =>
                    new AssetCategoryTotalDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        Count = total.Count,
                        TotalPurchaseCost = total.Cost,
                        TotalCurrentValue = total.Value,
                        TotalDepreciation = total.Cost - total.Value,
                    }
            )
            .OrderByDescending(c => c.TotalCurrentValue)
            .ThenBy(c => c.CategoryName);

    internal static IQueryable<AssetMonthRow> AcquiredByMonth(this IQueryable<Asset> assets) =>
        assets
            .Where(a => a.PurchaseDate != null)
            .GroupBy(a => new { a.PurchaseDate!.Value.Year, a.PurchaseDate.Value.Month })
            .Select(g => new AssetMonthRow(
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Sum(a => a.PurchaseCost)
            ));

    internal static IQueryable<AssetMonthRow> LostByMonth(this IQueryable<Asset> assets) =>
        assets
            .Where(a => a.LiquidatedOn != null)
            .GroupBy(a => new { a.LiquidatedOn!.Value.Year, a.LiquidatedOn.Value.Month })
            .Select(g => new AssetMonthRow(
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Sum(a => a.PurchaseCost)
            ));

    internal static IQueryable<Asset> Filter(
        this IQueryable<Asset> query,
        AssetFilters filters
    )
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.Name.Trim())}%";
            query = query.Where(a => EF.Functions.ILike(a.Name, pattern));
        }

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
        // The leading >= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every earlier name.
        if (cursor is not null)
            query = query.Where(a =>
                string.Compare(a.Name, cursor.Name) >= 0
                && (string.Compare(a.Name, cursor.Name) > 0 || a.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
