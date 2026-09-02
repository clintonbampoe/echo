using Echo.Application.Extensions.QueryMethods;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

    public virtual async Task<bool> UpdateRecord(
        Guid id,
        Guid congregationId,
        T entity,
        CancellationToken ct = default
    )
    {
        var existing = await DbSet
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id && e.CongregationId == congregationId, ct);
        if (existing is null)
            return false;

        DbSet.Entry(existing).CurrentValues.SetValues(entity);

        foreach (var property in DbSet.Entry(existing).Properties)
        {
            if (property.Metadata.ValueGenerated != ValueGenerated.Never)
                property.IsModified = false;
        }

        return true;
    }

    public virtual async Task<bool> DeleteRecord(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var existing = await DbSet
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id && e.CongregationId == congregationId, ct);

        if (existing is null)
            return false;

        existing.DeletedAt = DateTime.UtcNow;
        return true;
    }
}
