using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AssetCategoryRepository(AppDbContext context)
{
    private readonly DbSet<AssetCategory> _dbSet = context.Set<AssetCategory>();

    public async Task<List<AssetCategory>> GetAll(Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(c => c.CongregationId == congregationId)
            .ToListAsync(ct);
    }

    public async Task<AssetCategory?> GetById(Guid congregationId, int id, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(c => c.Id == id && c.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<AssetCategory>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(a => a.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(AssetCategory entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(AssetCategory entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}
