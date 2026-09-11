using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AttendanceRepository(AppDbContext context)
{
    private readonly DbSet<Attendance> _dbSet = context.Set<Attendance>();

    public async Task<List<Attendance>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        Parameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(a => a.CongregationId == congregationId);

        var res = await query
            .OrderBy(a => a.Id)
            .Include(a => a.AttendanceContext)
            .Include(a => a.Member)
            .ToListAsync(ct);
        return res;
    }

    public async Task<Attendance?> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(a => a.Id == id && a.CongregationId == congregationId)
            .Include(a => a.Member)
            .Include(a => a.AttendanceContext)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(Attendance entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Attendance entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}
