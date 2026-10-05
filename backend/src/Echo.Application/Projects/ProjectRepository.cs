using Echo.Data;
using Echo.Domain.Projects;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Projects;

public class ProjectRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<Project> _dbSet = context.Set<Project>();
    private readonly DbSet<ProjectContribution> _contributionDbSet =
        context.Set<ProjectContribution>();

    public async Task<List<Project>> List(
        Guid congregationId,
        ProjectFilters filters,
        ProjectCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Include(p => p.Category)
            .Include(p => p.Manager)
            .Filter(filters, timeProvider)
            .OrderBy(p => p.StartDate)
            .ThenBy(p => p.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Project?> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .Include(p => p.Category)
            .Include(p => p.Manager)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Project>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public Task<int> Count(
        Guid congregationId,
        ProjectFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<decimal> SumTarget(
        Guid congregationId,
        ProjectFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .SumAsync(p => p.TargetAmount, ct);

    public Task<decimal> SumRaised(
        Guid congregationId,
        ProjectFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .SelectMany(p => _contributionDbSet.FilterDeleted().Where(c => c.ProjectId == p.Id))
            .SumAsync(c => c.Amount, ct);

    public Task<int> CountAtRisk(
        Guid congregationId,
        ProjectFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId && p.Status == ProjectStatus.AtRisk)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public void Create(Project entity) => _dbSet.Add(entity);

    public void SoftDelete(Project entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class ProjectQueryExtensions
{
    internal static IQueryable<Project> Filter(
        this IQueryable<Project> query,
        ProjectFilters filters,
        TimeProvider timeProvider
    )
    {
        if (filters.Name is not null)
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{filters.Name}%"));

        if (filters.CategoryId is not null)
            query = query.Where(p => p.CategoryId == filters.CategoryId);

        if (filters.Status is not null)
            query = query.Where(p => p.Status == filters.Status);

        var from = filters.From ?? DateUtils.GetFirstDayOfWeek(timeProvider);
        var to = filters.To ?? DateUtils.GetDateToday(timeProvider);

        query = query.Where(p => p.StartDate >= from && p.StartDate <= to);

        return query;
    }

    internal static IQueryable<Project> Paginate(
        this IQueryable<Project> query,
        ProjectCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(p =>
                p.StartDate > cursor.StartDate
                || (p.StartDate == cursor.StartDate && p.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
