using Echo.Data;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Shared.Utilities;

namespace Echo.Application.Projects;

public class ProjectContributionRepository(AppDbContext context)
{
    private readonly DbSet<ProjectContribution> _dbSet = context.Set<ProjectContribution>();

    public async Task<List<ProjectContribution>> List(
        Guid congregationId,
        ProjectContributionFilters filters,
        ProjectContributionCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Include(p => p.Project)
            .Filter(filters)
            .OrderByDescending(p => p.DateContributed)
            .ThenBy(p => p.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<ProjectContribution?> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .Include(p => p.Project)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ContributionStats> GetSummaryStats(
        Guid congregationId,
        ProjectContributionFilters filters,
        CancellationToken ct = default
    )
    {
        var rows = await Scope(congregationId).Filter(filters).TotalsByMethod().ToListAsync(ct);
        return ContributionStats.From(rows);
    }

    public async Task<List<ContributionMonthRow>> GetMonthlyTotals(
        Guid congregationId,
        ProjectContributionMonthlyFilters filters,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var gifts = Scope(congregationId)
            .Filter(
                new ProjectContributionFilters
                {
                    ProjectId = filters.ProjectId,
                    PaymentMethod = filters.PaymentMethod,
                    From = from,
                    To = to,
                }
            );

        if (filters.CategoryId is not null)
        {
            var projects = context
                .Set<Project>()
                .Where(p => p.CategoryId == filters.CategoryId);
            gifts = gifts.Where(c => projects.Any(p => p.Id == c.ProjectId));
        }

        return await gifts.ByMonth().ToListAsync(ct);
    }

    private IQueryable<ProjectContribution> Scope(Guid congregationId) =>
        _dbSet.AsNoTracking().FilterDeleted().Where(c => c.CongregationId == congregationId);

    public void Create(ProjectContribution entity) => _dbSet.Add(entity);

    public void SoftDelete(ProjectContribution entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record ContributionStats(
    decimal Total,
    int Count,
    decimal Average,
    PaymentMethod? MostUsedPaymentMethod
)
{
    // The figures are worked out from the per-method totals, so one grouped scan answers all of them.
    // A tie for the most used method goes to the method that sorts first, so the answer is stable.
    public static ContributionStats From(IReadOnlyList<MethodTotal> rows)
    {
        var total = rows.Sum(r => r.Total);
        var count = rows.Sum(r => r.Count);
        var mostUsed = rows
            .OrderByDescending(r => r.Count)
            .ThenBy(r => r.Method)
            .Select(r => (PaymentMethod?)r.Method)
            .FirstOrDefault();

        return new ContributionStats(total, count, count == 0 ? 0m : total / count, mostUsed);
    }
}

public record MethodTotal(PaymentMethod Method, int Count, decimal Total);

public record ContributionMonthRow(int Year, int Month, decimal Total, int Count);

internal static class ProjectContributionQueryExtensions
{
    internal static IQueryable<MethodTotal> TotalsByMethod(
        this IQueryable<ProjectContribution> gifts
    ) =>
        gifts
            .GroupBy(c => c.PaymentMethod)
            .Select(g => new MethodTotal(g.Key, g.Count(), g.Sum(c => c.Amount)));

    internal static IQueryable<ContributionMonthRow> ByMonth(
        this IQueryable<ProjectContribution> gifts
    ) =>
        gifts
            .GroupBy(c => new { c.DateContributed.Year, c.DateContributed.Month })
            .Select(g => new ContributionMonthRow(
                g.Key.Year,
                g.Key.Month,
                g.Sum(c => c.Amount),
                g.Count()
            ));

    // Money given in the window and in the one before it (the two are side by side), in one scan.
    internal static IQueryable<RaisedInWindows> RaisedInWindows(
        this IQueryable<ProjectContribution> gifts,
        DateWindow window,
        DateWindow previous
    ) =>
        gifts
            .Where(c => c.DateContributed >= previous.From && c.DateContributed <= window.To)
            .GroupBy(_ => 1)
            .Select(g => new RaisedInWindows(
                g.Where(c => c.DateContributed >= window.From).Sum(c => c.Amount),
                g.Where(c => c.DateContributed <= previous.To).Sum(c => c.Amount)
            ));

    internal static IQueryable<ProjectContribution> Filter(
        this IQueryable<ProjectContribution> query,
        ProjectContributionFilters filters
    )
    {
        if (filters.ProjectId is not null)
            query = query.Where(p => p.ProjectId == filters.ProjectId);

        if (filters.MinAmount is not null)
            query = query.Where(p => p.Amount >= filters.MinAmount);

        if (filters.MaxAmount is not null)
            query = query.Where(p => p.Amount <= filters.MaxAmount);

        if (filters.PaymentMethod is not null)
            query = query.Where(p => p.PaymentMethod == filters.PaymentMethod);

        if (filters.From is not null)
            query = query.Where(p => p.DateContributed >= filters.From);

        if (filters.To is not null)
            query = query.Where(p => p.DateContributed <= filters.To);

        return query;
    }

    internal static IQueryable<ProjectContribution> Paginate(
        this IQueryable<ProjectContribution> query,
        ProjectContributionCursor? cursor,
        int pageSize
    )
    {
        // The leading <= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every newer gift.
        if (cursor is not null)
            query = query.Where(p =>
                p.DateContributed <= cursor.DateContributed
                && (p.DateContributed < cursor.DateContributed || p.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
