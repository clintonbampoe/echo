using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Attendances;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Attendances;

public class AttendanceContextRepository(AppDbContext context)
{
    private readonly DbSet<AttendanceContext> _dbSet =
        context.Set<AttendanceContext>();

    public async Task<List<AttendanceContext>> GetAll(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
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
            .FilterDeleted()
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
            .FilterDeleted()
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
