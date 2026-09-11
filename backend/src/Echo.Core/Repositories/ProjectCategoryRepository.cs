using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class ProjectCategoryRepository(AppDbContext context)
{
    private readonly DbSet<ProjectCategory> _dbSet = context.Set<ProjectCategory>();

    public async Task<List<ProjectCategory>> GetAll(Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
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
            .FilterSoftDeleted()
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
            .FilterSoftDeleted()
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
