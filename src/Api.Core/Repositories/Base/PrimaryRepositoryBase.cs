using Api.Core.Data;
using Api.Core.Common.ExtensionMethods;
using Api.Core.Entities.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories.Base;

public abstract class PrimaryRepositoryBase<T>(AppDbContext context)
    where T : class, IPrimaryEntity
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<bool> CreateRecord(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        return true;
    }

    public virtual async Task<bool> UpdateRecord(Guid id, T entity, CancellationToken ct = default)
    {
        var existing = await _dbSet
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (existing is null)
            return false;

        _dbSet.Entry(existing).CurrentValues.SetValues(entity);
        return true;
    }

    public virtual async Task<bool> DeleteRecord(Guid id, CancellationToken ct = default)
    {
        var existing = await _dbSet
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (existing is null)
            return false;

        existing.DeletedAt = DateTime.UtcNow;
        return true;
    }
}
