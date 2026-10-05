using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Attendances;

public class AttendanceTypeRepository(AppDbContext context)
{
    private readonly DbSet<AttendanceType> _dbSet = context.Set<AttendanceType>();

    public async Task<List<AttendanceType>> List(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(s => s.CongregationId == congregationId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<AttendanceType?> GetById(
        Guid congregationId,
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(s => s.Id == id && s.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<AttendanceType>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(s => s.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(AttendanceType entity) => _dbSet.Add(entity);

    public void SoftDelete(AttendanceType entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class ServiceTypeQueryExtensions
{
    internal static IQueryable<AttendanceType> SearchName(
        this IQueryable<AttendanceType> query,
        string name
    )
    {
        return query
            .Where(s => EF.Functions.ILike(s.Name, $"%{name}%"))
            .OrderByDescending(s => EF.Functions.TrigramsSimilarity(s.Name, name))
            .Take(5);
    }
}
