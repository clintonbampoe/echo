using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Projects;
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

    public void Create(ProjectCategory entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(ProjectCategory entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}
