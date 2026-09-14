using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

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
            .FilterSoftDeleted()
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
            .FilterSoftDeleted()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .Include(p => p.Project)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(ProjectContribution entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(ProjectContribution entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class ProjectContributionQueryExtensions
{
    internal static IQueryable<ProjectContribution> Filter(
        this IQueryable<ProjectContribution> query,
        ProjectContributionFilters filters
    )
    {
        if (filters.Amount is not null)
            query = query.Where(p => p.Amount > filters.Amount);

        if (filters.Date is not null)
            query = query.Where(p => p.DateContributed == filters.Date);

        if (filters.PaymentMethod is not null)
            query = query.Where(p => p.PaymentMethod == filters.PaymentMethod);

        return query;
    }

    internal static IQueryable<ProjectContribution> Paginate(
        this IQueryable<ProjectContribution> query,
        ProjectContributionCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(e =>
                e.DateContributed < cursor.DateContributed
                || (e.DateContributed == cursor.DateContributed && e.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
