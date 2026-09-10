using Echo.Application.Extensions.QueryExtensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class CongregationRepository(AppDbContext context)
{
    private readonly DbSet<Congregation> _dbSet = context.Set<Congregation>();

    public async Task<Congregation?> GetById(
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(Congregation entity)
    {
        _dbSet.Add(entity);
    }

    public void Delete(Congregation entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}
