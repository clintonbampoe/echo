using Echo.Data;
using Echo.Domain.Projects;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

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

    // The project ids come from a query already limited to the congregation, so the gifts are not
    // limited again: a congregation filter would send the database to the table for every row.
    public async Task<Dictionary<Guid, ProjectContributionTotal>> GetContributionTotals(
        IReadOnlyList<Guid> projectIds,
        CancellationToken ct = default
    )
    {
        if (projectIds.Count == 0)
            return [];

        return await _contributionDbSet
            .AsNoTracking()
            .ContributionTotalsPerProject(projectIds)
            .ToDictionaryAsync(x => x.Id, ct);
    }

    public async Task<ProjectStats> GetSummaryStats(
        Guid congregationId,
        ProjectFilters filters,
        CancellationToken ct = default
    )
    {
        var stats = await Scope(congregationId, filters)
            .SummarizeProjects(Gifts(congregationId))
            .FirstOrDefaultAsync(ct);
        return stats ?? new ProjectStats(0, 0m, 0m, 0);
    }

    // Money given in the window and in the one before it, to the projects the filters pick. The
    // filters' dates are left out: they say when a project ran, not when money was given.
    public async Task<RaisedInWindows> GetRaisedInWindows(
        Guid congregationId,
        ProjectFilters filters,
        DateWindow window,
        CancellationToken ct = default
    )
    {
        var previous = window.Previous();
        var projects = Scope(congregationId, filters with { From = null, To = null });

        var raised = await Gifts(congregationId)
            .Where(c => projects.Any(p => p.Id == c.ProjectId))
            .RaisedInWindows(window, previous)
            .FirstOrDefaultAsync(ct);
        return raised ?? new RaisedInWindows(0m, 0m);
    }

    public async Task<List<ProjectCategoryTotalDto>> GetCategoryTotals(
        Guid congregationId,
        ProjectFilters filters,
        CancellationToken ct = default
    ) =>
        await Scope(congregationId, filters)
            // Summed per project through the project index, so the gifts are not limited to the
            // congregation here either.
            .TotalsByCategory(context.Set<ProjectCategory>(), _contributionDbSet.FilterDeleted())
            .ToListAsync(ct);

    private IQueryable<Project> Scope(Guid congregationId, ProjectFilters filters) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Filter(filters);

    private IQueryable<ProjectContribution> Gifts(Guid congregationId) =>
        _contributionDbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(c => c.CongregationId == congregationId);

    public void Create(Project entity) => _dbSet.Add(entity);

    public void SoftDelete(Project entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record ProjectStats(int Count, decimal Raised, decimal Target, int AtRisk);

public record ProjectContributionTotal(Guid Id, decimal Amount, int Count);

public record RaisedInWindows(decimal Current, decimal Previous);

internal static class ProjectQueryExtensions
{
    internal static IQueryable<ProjectContributionTotal> ContributionTotalsPerProject(
        this IQueryable<ProjectContribution> query,
        IReadOnlyList<Guid> projectIds
    ) =>
        query
            .FilterDeleted()
            .Where(c => projectIds.Contains(c.ProjectId))
            .GroupBy(c => c.ProjectId)
            .Select(g => new ProjectContributionTotal(g.Key, g.Sum(c => c.Amount), g.Count()));

    // One pass over the projects for every figure; what was raised is the gifts to those projects,
    // summed once rather than joined to each project.
    internal static IQueryable<ProjectStats> SummarizeProjects(
        this IQueryable<Project> projects,
        IQueryable<ProjectContribution> gifts
    ) =>
        projects
            .GroupBy(p => p.CongregationId)
            .Select(group => new ProjectStats(
                group.Count(),
                gifts.Where(c => projects.Any(p => p.Id == c.ProjectId)).Sum(c => c.Amount),
                group.Sum(p => p.TargetAmount),
                group.Count(p => p.Status == ProjectStatus.AtRisk)
            ));

    internal static IQueryable<ProjectCategoryTotalDto> TotalsByCategory(
        this IQueryable<Project> projects,
        IQueryable<ProjectCategory> categories,
        IQueryable<ProjectContribution> gifts
    ) =>
        projects
            // Each project's raised amount is one index-only sum over its own gifts; the categories
            // then add up the projects.
            .Select(p => new
            {
                p.CategoryId,
                p.TargetAmount,
                Raised = gifts.Where(c => c.ProjectId == p.Id).Sum(c => c.Amount),
            })
            .GroupBy(x => x.CategoryId)
            .Select(group => new
            {
                CategoryId = group.Key,
                Count = group.Count(),
                Target = group.Sum(x => x.TargetAmount),
                Raised = group.Sum(x => x.Raised),
            })
            .Join(
                categories,
                total => total.CategoryId,
                category => category.Id,
                (total, category) =>
                    new ProjectCategoryTotalDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        ProjectCount = total.Count,
                        TotalTarget = total.Target,
                        TotalRaised = total.Raised,
                    }
            )
            .OrderByDescending(c => c.TotalRaised)
            .ThenBy(c => c.CategoryName);

    internal static IQueryable<Project> Filter(
        this IQueryable<Project> query,
        ProjectFilters filters
    )
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.Name.Trim())}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

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
        // The leading >= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every earlier project.
        if (cursor is not null)
            query = query.Where(p =>
                p.StartDate >= cursor.StartDate
                && (p.StartDate > cursor.StartDate || p.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
