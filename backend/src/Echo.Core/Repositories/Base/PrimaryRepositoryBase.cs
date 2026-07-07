using Echo.Domain.Data;
using Echo.Domain.Entities.Core.Interfaces;
using Echo.Application.Extensions.QueryMethods;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories.Base;

public abstract class PrimaryRepositoryBase<T>(AppDbContext context)
    where T : class, IPrimaryEntity
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<bool> CreateRecord(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        return true;
    }

    public virtual async Task<bool> UpdateRecord(Guid id, T entity, CancellationToken ct = default)
    {
        var existing = await DbSet
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (existing is null)
            return false;

        DbSet.Entry(existing).CurrentValues.SetValues(entity);
        DbSet.Entry(existing).Property(e => e.Id).IsModified = false;
        DbSet.Entry(existing).Property(e => e.CreatedAt).IsModified = false;
        DbSet.Entry(existing).Property(e => e.DeletedAt).IsModified = false;

        return true;
    }

    public virtual async Task<bool> DeleteRecord(Guid id, CancellationToken ct = default)
    {
        var existing = await DbSet
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (existing is null)
            return false;

        existing.DeletedAt = DateTime.UtcNow;
        return true;
    }
}
