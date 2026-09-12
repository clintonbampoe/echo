using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AttendanceTypeRepository(AppDbContext context)
{
    private readonly DbSet<AttendanceType> _dbSet = context.Set<AttendanceType>();

    public async Task<List<AttendanceType>> GetAll(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(t => t.CongregationId == congregationId)
            .ToListAsync(ct);
    }

    public async Task<AttendanceType?> GetById(
        Guid congregationId,
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<AttendanceType>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(at => at.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(AttendanceType entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(AttendanceType entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}
