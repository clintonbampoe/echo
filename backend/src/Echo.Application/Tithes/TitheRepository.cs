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
        MemberTitheFilters filters,
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

    // True when the member already has a (not deleted) tithe for that month. `exceptId` leaves out the tithe being
    // edited, so saving it again without moving it is not a clash with itself.
    public async Task<bool> ExistsForMonth(
        Guid congregationId,
        Guid memberId,
        int year,
        MonthOfYear month,
        Guid? exceptId,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterDeleted()
            .AnyAsync(
                t =>
                    t.CongregationId == congregationId
                    && t.MemberId == memberId
                    && t.ForYear == year
                    && t.ForMonth == month
                    && (exceptId == null || t.Id != exceptId),
                ct
            );
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

        if (!string.IsNullOrWhiteSpace(filters.MemberName))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.MemberName.Trim())}%";
            query = query.Where(t => EF.Functions.ILike(t.Member.Person.Name, pattern));
        }

        if (filters.PaymentMethod is not null)
            query = query.Where(t => t.PaymentMethod == filters.PaymentMethod);

        var rows = await query.InYears(first, last).MonthlyTotals().ToListAsync(ct);
        return rows.Select(r => r.ToDto()).ToList();
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

        var totals = await context
            .OneRow(congregationId)
            .SummarizeTithes(tithes)
            .FirstOrDefaultAsync(ct);
        var byMethod = await tithes.ByMethod().ToListAsync(ct);

        return new TitheStats(totals?.Total ?? 0m, totals?.UniqueTithers ?? 0, totals?.Largest ?? 0m, byMethod);
    }

    public void Create(Tithe entity) => _dbSet.Add(entity);

    public void SoftDelete(Tithe entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal record TitheMonthRow(int Year, MonthOfYear Month, decimal Total, int UniqueTithers);

public record TitheTotals(decimal Total, int UniqueTithers, decimal Largest);

public record MethodShare(PaymentMethod Method, int Count, decimal Total);

public record TitheStats(
    decimal Total,
    int UniqueTithers,
    decimal Largest,
    IReadOnlyList<MethodShare> ByMethod
)
{
    // The method most tithes were paid with; a tie goes to the method that sorts first, so the
    // answer is stable.
    public static PaymentMethod? MostUsed(IReadOnlyList<MethodShare> shares) =>
        shares
            .OrderByDescending(s => s.Count)
            .ThenBy(s => s.Method)
            .Select(s => (PaymentMethod?)s.Method)
            .FirstOrDefault();
}

internal static class TitheQueryExtensions
{
    internal static IQueryable<TitheTotals> SummarizeTithes(
        this IQueryable<Congregation> oneRow,
        IQueryable<Tithe> tithes
    ) =>
        oneRow.Select(_ => new TitheTotals(
            tithes.Sum(t => t.Amount),
            tithes.Select(t => t.MemberId).Distinct().Count(),
            tithes.Max(t => (decimal?)t.Amount) ?? 0m
        ));

    // How the tithes were paid: how many and how much for each method.
    internal static IQueryable<MethodShare> ByMethod(this IQueryable<Tithe> tithes) =>
        tithes
            .GroupBy(t => t.PaymentMethod)
            .Select(g => new MethodShare(g.Key, g.Count(), g.Sum(t => t.Amount)));

    // Narrows to the years the chart covers. The month is stored as text, so it cannot be compared or added to a
    // number in SQL; months outside the range come back as extra rows and FillMonths leaves them out.
    internal static IQueryable<Tithe> InYears(
        this IQueryable<Tithe> query,
        YearMonth first,
        YearMonth last
    ) => query.Where(t => t.ForYear >= first.Year && t.ForYear <= last.Year);

    // The month stays an enum here: it is stored as text, so turning it into a number happens after the query (ToDto).
    internal static IQueryable<TitheMonthRow> MonthlyTotals(this IQueryable<Tithe> query) =>
        query
            .GroupBy(t => new { t.ForYear, t.ForMonth })
            .Select(g => new TitheMonthRow(
                g.Key.ForYear,
                g.Key.ForMonth,
                g.Sum(t => t.Amount),
                // A member has at most one live tithe a month (unique index), so rows are tithers.
                g.Count()
            ));

    internal static TitheMonthlyDto ToDto(this TitheMonthRow row) =>
        new()
        {
            Year = row.Year,
            Month = (int)row.Month,
            TotalCollected = row.Total,
            UniqueTithers = row.UniqueTithers,
            AveragePerMember = TitheService.AveragePerMember(row.Total, row.UniqueTithers),
        };

    // Which members' tithes, then everything a single member's tithes can be narrowed by.
    internal static IQueryable<Tithe> Filter(this IQueryable<Tithe> query, TitheFilters filters)
    {
        if (filters.MemberId is not null)
            query = query.Where(t => t.MemberId == filters.MemberId);

        if (!string.IsNullOrWhiteSpace(filters.MemberName))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.MemberName.Trim())}%";
            query = query.Where(t => EF.Functions.ILike(t.Member.Person.Name, pattern));
        }

        return query.Filter((MemberTitheFilters)filters);
    }

    internal static IQueryable<Tithe> Filter(
        this IQueryable<Tithe> query,
        MemberTitheFilters filters
    )
    {
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
        // The leading <= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every newer row.
        if (cursor is not null)
            query = query.Where(t =>
                t.CollectionDate <= cursor.CollectionDate
                && (t.CollectionDate < cursor.CollectionDate || t.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
