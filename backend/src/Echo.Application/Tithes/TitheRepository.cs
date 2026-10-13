using Echo.Data;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Shared.Utilities;
using Echo.Domain.Congregations;

namespace Echo.Application.Tithes;

public class TitheRepository(AppDbContext context)
{
    private readonly DbSet<Tithe> _dbSet = context.Set<Tithe>();

    public async Task<List<Tithe>> List(
        Guid congregationId,
        TitheFilters filters,
        TitheCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Include(t => t.Member)
                .ThenInclude(m => m.Person)
            .Filter(filters)
            .OrderForList()
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Tithe?> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Include(t => t.Member)
                .ThenInclude(m => m.Person)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Tithe>> GetByMemberId(
        Guid congregationId,
        Guid memberId,
        TitheFilters filters,
        TitheCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(t => t.MemberId == memberId && t.CongregationId == congregationId)
            .Include(t => t.Member)
                .ThenInclude(t => t.Person)
            .Filter(filters)
            .OrderForList()
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<TitheMonthlyDto>> GetMonthlyTotals(
        Guid congregationId,
        TitheMonthlyFilters filters,
        YearMonth first,
        YearMonth last,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId);

        if (filters.MemberId is not null)
            query = query.Where(t => t.MemberId == filters.MemberId);

        if (filters.PaymentMethod is not null)
            query = query.Where(t => t.PaymentMethod == filters.PaymentMethod);

        return await query.InMonths(first, last).MonthlyTotals().ToListAsync(ct);
    }

    public async Task<TitheStats> GetSummaryStats(
        Guid congregationId,
        TitheFilters filters,
        CancellationToken ct = default
    )
    {
        var tithes = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters);

        var stats = await context.OneRow(congregationId).SummarizeTithes(tithes).FirstOrDefaultAsync(ct);
        return stats ?? new TitheStats(0m, 0, 0m, null);
    }

    public void Create(Tithe entity) => _dbSet.Add(entity);

    public void SoftDelete(Tithe entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class TitheQueryExtensions
{
    internal static IQueryable<TitheStats> SummarizeTithes(
        this IQueryable<Congregation> oneRow,
        IQueryable<Tithe> tithes
    ) =>
        oneRow.Select(_ => new TitheStats(
            tithes.Sum(t => t.Amount),
            tithes.Select(t => t.MemberId).Distinct().Count(),
            tithes.Average(t => (decimal?)t.Amount) ?? 0m,
            tithes
                .GroupBy(t => t.PaymentMethod)
                .OrderByDescending(g => g.Count())
                .Select(g => (PaymentMethod?)g.Key)
                .FirstOrDefault()
        ));

    // Tithes are charted by the month they are for (ForYear and ForMonth).
    internal static IQueryable<Tithe> InMonths(
        this IQueryable<Tithe> query,
        YearMonth first,
        YearMonth last
    )
    {
        var firstIndex = first.Year * 12 + first.Month;
        var lastIndex = last.Year * 12 + last.Month;
        return query.Where(t =>
            t.ForYear * 12 + (int)t.ForMonth >= firstIndex
            && t.ForYear * 12 + (int)t.ForMonth <= lastIndex
        );
    }

    internal static IQueryable<TitheMonthlyDto> MonthlyTotals(this IQueryable<Tithe> query) =>
        query
            .GroupBy(t => new { t.ForYear, t.ForMonth })
            .Select(g => new TitheMonthlyDto
            {
                Year = g.Key.ForYear,
                Month = (int)g.Key.ForMonth,
                TotalCollected = g.Sum(t => t.Amount),
                UniqueTithers = g.Select(t => t.MemberId).Distinct().Count(),
            });

    internal static IQueryable<Tithe> Filter(
        this IQueryable<Tithe> query,
        TitheFilters filters
    )
    {
        if (filters.MemberId is not null)
            query = query.Where(t => t.MemberId == filters.MemberId);

        if (filters.PaymentMethod is not null)
            query = query.Where(t => t.PaymentMethod == filters.PaymentMethod);

        if (filters.Year is not null)
            query = query.Where(t => t.ForYear == filters.Year);

        if (filters.Month is not null)
            query = query.Where(t => t.ForMonth == filters.Month);

        if (filters.From is not null)
            query = query.Where(t => t.CollectionDate >= filters.From);

        if (filters.To is not null)
            query = query.Where(t => t.CollectionDate <= filters.To);

        return query;
    }

    // Paginate's cursor assumes exactly this order.
    internal static IOrderedQueryable<Tithe> OrderForList(this IQueryable<Tithe> query) =>
        query.OrderByDescending(t => t.CollectionDate).ThenBy(t => t.Id);

    internal static IQueryable<Tithe> Paginate(
        this IQueryable<Tithe> query,
        TitheCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(t =>
                t.CollectionDate < cursor.CollectionDate
                || (t.CollectionDate == cursor.CollectionDate && t.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
