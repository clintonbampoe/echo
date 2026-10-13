using Echo.Data;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Domain.Congregations;

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
        var contributions = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(c => c.CongregationId == congregationId)
            .Filter(filters);

        var stats = await context
            .OneRow(congregationId)
            .SummarizeContributions(contributions)
            .FirstOrDefaultAsync(ct);
        return stats ?? new ContributionStats(0m, 0, 0m, null);
    }

    public void Create(ProjectContribution entity) => _dbSet.Add(entity);

    public void SoftDelete(ProjectContribution entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record ContributionStats(
    decimal Total,
    int Count,
    decimal Average,
    PaymentMethod? MostUsedPaymentMethod
);

internal static class ProjectContributionQueryExtensions
{
    internal static IQueryable<ContributionStats> SummarizeContributions(
        this IQueryable<Congregation> oneRow,
        IQueryable<ProjectContribution> contributions
    ) =>
        oneRow.Select(_ => new ContributionStats(
            contributions.Sum(c => c.Amount),
            contributions.Count(),
            contributions.Average(c => (decimal?)c.Amount) ?? 0m,
            contributions
                .GroupBy(c => c.PaymentMethod)
                .OrderByDescending(g => g.Count())
                .Select(g => (PaymentMethod?)g.Key)
                .FirstOrDefault()
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
        if (cursor is not null)
            query = query.Where(p =>
                p.DateContributed < cursor.DateContributed
                || (p.DateContributed == cursor.DateContributed && p.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
