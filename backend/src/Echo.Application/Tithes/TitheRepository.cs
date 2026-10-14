using System.Linq.Expressions;
using Echo.Data;
using Echo.Domain.Congregations;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

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

        var totals = await query.InMonths(first, last).MonthlyTotals().ToListAsync(ct);
        return totals.Select(t => t.ToDto()).ToList();
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

        var stats = await context
            .OneRow(congregationId)
            .SummarizeTithes(tithes)
            .FirstOrDefaultAsync(ct);
        return stats ?? new TitheStats(0m, 0, null);
    }

    public void Create(Tithe entity) => _dbSet.Add(entity);

    public void SoftDelete(Tithe entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record TitheStats(decimal Total, int UniqueTithers, PaymentMethod? MostUsedPaymentMethod);

internal record TitheMonthTotal(
    int Year,
    MonthOfYear Month,
    decimal TotalCollected,
    int UniqueTithers
);

internal static class TitheQueryExtensions
{
    internal static IQueryable<TitheStats> SummarizeTithes(
        this IQueryable<Congregation> oneRow,
        IQueryable<Tithe> tithes
    ) =>
        oneRow.Select(_ => new TitheStats(
            tithes.Sum(t => t.Amount),
            tithes.Select(t => t.MemberId).Distinct().Count(),
            tithes
                .GroupBy(t => t.PaymentMethod)
                .OrderByDescending(g => g.Count())
                .Select(g => (PaymentMethod?)g.Key)
                .FirstOrDefault()
        ));

    // Tithes are charted by the month they are for (ForYear and ForMonth).
    // ForMonth is stored as the month's name, so the database cannot do arithmetic on it. Each wanted
    // month is matched on its own (year equals, month equals), which works in SQL and in memory alike.
    internal static IQueryable<Tithe> InMonths(
        this IQueryable<Tithe> query,
        YearMonth first,
        YearMonth last
    )
    {
        var firstIndex = first.Year * 12 + first.Month - 1;
        var lastIndex = last.Year * 12 + last.Month - 1;

        var t = Expression.Parameter(typeof(Tithe), "t");
        Expression wanted = Enumerable
            .Range(firstIndex, Math.Max(0, lastIndex - firstIndex + 1))
            .Select(index =>
                (Expression)
                    Expression.AndAlso(
                        Expression.Equal(
                            Expression.Property(t, nameof(Tithe.ForYear)),
                            Expression.Constant(index / 12)
                        ),
                        Expression.Equal(
                            Expression.Property(t, nameof(Tithe.ForMonth)),
                            Expression.Constant((MonthOfYear)(index % 12 + 1))
                        )
                    )
            )
            .Aggregate((Expression)Expression.Constant(false), Expression.OrElse);

        return query.Where(Expression.Lambda<Func<Tithe, bool>>(wanted, t));
    }

    // The month stays an enum until the rows are read: turning it into its number is done in memory.
    internal static IQueryable<TitheMonthTotal> MonthlyTotals(this IQueryable<Tithe> query) =>
        query
            .GroupBy(t => new { t.ForYear, t.ForMonth })
            .Select(g => new TitheMonthTotal(
                g.Key.ForYear,
                g.Key.ForMonth,
                g.Sum(t => t.Amount),
                g.Select(t => t.MemberId).Distinct().Count()
            ));

    internal static TitheMonthlyDto ToDto(this TitheMonthTotal total) =>
        new()
        {
            Year = total.Year,
            Month = (int)total.Month,
            TotalCollected = total.TotalCollected,
            UniqueTithers = total.UniqueTithers,
        };

    internal static IQueryable<Tithe> Filter(this IQueryable<Tithe> query, TitheFilters filters)
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
