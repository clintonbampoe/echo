using Echo.Data;
using Echo.Domain.Projects;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Projects;

public class ProjectCategoryRepository(AppDbContext context)
{
    private readonly DbSet<ProjectCategory> _dbSet = context.Set<ProjectCategory>();

    public async Task<List<ProjectCategory>> GetAll(Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(c => c.CongregationId == congregationId)
            .ToListAsync(ct);
    }

    public async Task<ProjectCategory?> GetById(
        Guid congregationId,
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(c => c.Id == id && c.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ProjectCategory>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(pc => pc.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<int, int>> GetProjectCounts(
        Guid congregationId,
        IReadOnlyList<int> categoryIds,
        CancellationToken ct = default
    )
    {
        if (categoryIds.Count == 0)
            return [];

        return await context
            .Set<Project>()
            .AsNoTracking()
            .Where(p => p.CongregationId == congregationId)
            .CountPerProjectCategory(categoryIds)
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
    }

    public void Create(ProjectCategory entity)
    {
        _dbSet.Add(entity);
    }

    public void BatchCreate(IList<ProjectCategory> entities)
    {
        _dbSet.AddRange(entities);
    }
  
    public async Task<bool> IsInUse(Guid congregationId, int id, CancellationToken ct)
    {
        return await context
            .Set<Project>()
            .FilterDeleted()
            .AnyAsync(e => e.CongregationId == congregationId && e.CategoryId == id, ct);
    }

    public void SoftDelete(ProjectCategory entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class ProjectCategoryQueryExtensions
{
    internal static IQueryable<CategoryCount> CountPerProjectCategory(
        this IQueryable<Project> query,
        IReadOnlyList<int> categoryIds
    ) =>
        query
            .FilterDeleted()
            .Where(p => categoryIds.Contains(p.CategoryId))
            .GroupBy(p => p.CategoryId)
            .Select(g => new CategoryCount(g.Key, g.Count()));

}
