using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Domain.Members;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Attendances;

public class AttendanceRepository(AppDbContext context, TimeProvider timeProvider)
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
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Include(a => a.Person)
            .Include(a => a.AttendanceType)
            .Filter(filters, timeProvider)
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Attendance?> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(a => a.Id == id && a.CongregationId == congregationId)
            .Include(a => a.Person)
            .Include(a => a.AttendanceType)
            .FirstOrDefaultAsync(ct);
    }

    public Task<int> CountTotal(
        Guid congregationId,
        AttendanceFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<int> CountByKind(
        Guid congregationId,
        PersonKind kind,
        AttendanceFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId && a.Person.Kind == kind)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<int> CountFirstTimeVisitors(
        Guid congregationId,
        AttendanceFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId && a.Person.Kind == PersonKind.Visitor)
            .Filter(filters, timeProvider)
            .Where(a =>
                !_dbSet
                    .FilterDeleted()
                    .Any(prev => prev.PersonId == a.PersonId && prev.Date < a.Date)
            )
            .Select(a => a.PersonId)
            .Distinct()
            .CountAsync(ct);

    public void Create(Attendance entity) => _dbSet.Add(entity);

    public void SoftDelete(Attendance entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class AttendanceQueryExtensions
{
    internal static IQueryable<Attendance> Filter(
        this IQueryable<Attendance> query,
        AttendanceFilters filters,
        TimeProvider timeProvider
    )
    {
        if (filters.AttendanceTypeId is not null)
            query = query.Where(a => a.AttendanceTypeId == filters.AttendanceTypeId);

        if (filters.Kind is not null)
            query = query.Where(a => a.Person.Kind == filters.Kind);

        var from = filters.From ?? DateUtils.GetFirstDayOfWeek(timeProvider);
        var to = filters.To ?? DateUtils.GetDateToday(timeProvider);

        query = query.Where(a => a.Date >= from && a.Date <= to);

        return query;
    }

    internal static IQueryable<Attendance> Paginate(
        this IQueryable<Attendance> query,
        AttendanceCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(a =>
                a.Date < cursor.Date || (a.Date == cursor.Date && a.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
