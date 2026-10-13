using Echo.Data;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

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

    public Task<decimal> SumContributed(
        Guid congregationId,
        ProjectContributionFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters)
            .SumAsync(p => p.Amount, ct);

    public Task<int> Count(
        Guid congregationId,
        ProjectContributionFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters)
            .CountAsync(ct);

    public Task<decimal> Average(
        Guid congregationId,
        ProjectContributionFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters)
            .AverageAsync(p => (decimal?)p.Amount, ct)
            .ContinueWith(t => t.Result ?? 0m, ct);

    public Task<PaymentMethod?> MostUsedPaymentMethod(
        Guid congregationId,
        ProjectContributionFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters)
            .GroupBy(p => p.PaymentMethod)
            .OrderByDescending(g => g.Count())
            .Select(g => (PaymentMethod?)g.Key)
            .FirstOrDefaultAsync(ct);

    public void Create(ProjectContribution entity) => _dbSet.Add(entity);

    public void SoftDelete(ProjectContribution entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class ProjectContributionQueryExtensions
{
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
