using Echo.Data;
using Echo.Domain.Projects;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Domain.Congregations;

namespace Echo.Application.Projects;

public class ProjectRepository(AppDbContext context)
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
                .ThenInclude(m => m.Person)
            .Filter(filters)
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
                .ThenInclude(m => m.Person)
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

    public async Task<Dictionary<Guid, decimal>> GetRaisedAmounts(
        Guid congregationId,
        IReadOnlyList<Guid> projectIds,
        CancellationToken ct = default
    )
    {
        if (projectIds.Count == 0)
            return [];

        return await _contributionDbSet
            .AsNoTracking()
            .Where(c => c.CongregationId == congregationId)
            .RaisedPerProject(projectIds)
            .ToDictionaryAsync(x => x.Id, x => x.Amount, ct);
    }

    public async Task<ProjectStats> GetSummaryStats(
        Guid congregationId,
        ProjectFilters filters,
        CancellationToken ct = default
    )
    {
        var projects = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters);

        var stats = await context
            .OneRow(congregationId)
            .SummarizeProjects(projects, _contributionDbSet.FilterDeleted())
            .FirstOrDefaultAsync(ct);
        return stats ?? new ProjectStats(0, 0m, 0m, 0);
    }

    public void Create(Project entity) => _dbSet.Add(entity);

    public void SoftDelete(Project entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record ProjectStats(int Count, decimal Raised, decimal Target, int AtRisk);

internal readonly record struct ParentAmount(Guid Id, decimal Amount);

internal static class ProjectQueryExtensions
{
    internal static IQueryable<ParentAmount> RaisedPerProject(
        this IQueryable<ProjectContribution> query,
        IReadOnlyList<Guid> projectIds
    ) =>
        query
            .FilterDeleted()
            .Where(c => projectIds.Contains(c.ProjectId))
            .GroupBy(c => c.ProjectId)
            .Select(g => new ParentAmount(g.Key, g.Sum(c => c.Amount)));

    internal static IQueryable<ProjectStats> SummarizeProjects(
        this IQueryable<Congregation> oneRow,
        IQueryable<Project> projects,
        IQueryable<ProjectContribution> contributions
    ) =>
        oneRow.Select(_ => new ProjectStats(
            projects.Count(),
            projects.SelectMany(p => contributions.Where(c => c.ProjectId == p.Id)).Sum(c => c.Amount),
            projects.Sum(p => p.TargetAmount),
            projects.Count(p => p.Status == ProjectStatus.AtRisk)
        ));

    internal static IQueryable<Project> Filter(
        this IQueryable<Project> query,
        ProjectFilters filters
    )
    {
        if (filters.Name is not null)
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{filters.Name}%"));

        if (filters.CategoryId is not null)
            query = query.Where(p => p.CategoryId == filters.CategoryId);

        if (filters.ManagerId is not null)
            query = query.Where(p => p.ManagerId == filters.ManagerId);

        if (filters.Status is not null)
            query = query.Where(p => p.Status == filters.Status);

        // A project is in the window when it started on or before To
        // and has not ended before From (an open-ended project never ends).
        if (filters.To is not null)
            query = query.Where(p => p.StartDate <= filters.To);

        if (filters.From is not null)
            query = query.Where(p => p.EndDate == null || p.EndDate >= filters.From);

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
