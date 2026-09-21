using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Congregations;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Congregations;

public class CongregationRepository(AppDbContext context)
{
    private readonly DbSet<Congregation> _dbSet = context.Set<Congregation>();

    public async Task<Congregation?> GetById(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FilterDeleted().Where(c => c.Id == id).FirstOrDefaultAsync(ct);
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
