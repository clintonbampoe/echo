using Echo.Application.Extensions.QueryMethods;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories.Base;

public abstract class ReferenceRepositoryBase<T>(AppDbContext context)
    where T : class, IReferenceEntity
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetEntityById(
        Guid congregationId,
        int id,
        CancellationToken ct = default)
    {
        return await DbSet
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id && e.CongregationId == congregationId, ct);
    }

    public virtual async Task Create(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
    }

    public virtual Task SoftDelete(T entity, CancellationToken ct = default)
    {
        entity.DeletedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }
}
