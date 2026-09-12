using Echo.Application.Query.Extensions;
using Echo.Application.Utilities;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AttendanceRepository(AppDbContext context)
{
    private readonly DbSet<Attendance> _dbSet = context.Set<Attendance>();

    public async Task<List<Attendance>> List(
        Guid congregationId,
        AttendanceFilters filters,
        AttendanceCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Include(a => a.AttendanceContext)
            .Include(a => a.Member)
            .Filter(filters)
            .OrderByDescending(a => a.ForDate)
            .ThenBy(a => a.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
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

internal static class AttendanceQueryExtensions
{
    internal static IQueryable<Attendance> Filter(
        this IQueryable<Attendance> query,
        AttendanceFilters filters
    )
    {
        var firstDayOfWeek = DateUtils.GetFirstDayOfWeek(TimeProvider.System.GetUtcNow().DateTime);

        query = filters.ForDate is not null
            ? query.Where(a => a.ForDate == filters.ForDate)
            : query.Where(a => a.ForDate >= firstDayOfWeek);

        if (filters.AttendanceContextId is not null)
            query = query.Where(a => a.AttendanceContextId == filters.AttendanceContextId);

        if (filters.MemberId is not null)
            query = query.Where(a => a.MemberId == filters.MemberId);

        if (filters.MemberName is not null)
            query = query.Where(a => EF.Functions.ILike(a.Member.Name, filters.MemberName));

        return query;
    }

    internal static IQueryable<Attendance> Paginate(
        this IQueryable<Attendance> query,
        AttendanceCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(e =>
                e.ForDate < cursor.ForDate || (e.ForDate == cursor.ForDate && e.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
