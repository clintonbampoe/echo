using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AttendanceContextRepository(AppDbContext context)
{
    private readonly DbSet<AttendanceContext> _dbSet = context.Set<AttendanceContext>();

    public async Task<List<AttendanceContext>> GetAll(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(c => c.CongregationId == congregationId)
            .ToListAsync(ct);
    }

    public async Task<AttendanceContext?> GetById(
        Guid congregationId,
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(c => c.Id == id && c.CongregationId == congregationId)
            .Include(c => c.AttendanceType)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<AttendanceContext>> Search(
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

    public void Create(AttendanceContext entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(AttendanceContext entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}
