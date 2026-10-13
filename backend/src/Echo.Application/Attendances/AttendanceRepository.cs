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
            .Include(a => a.Session)
                .ThenInclude(s => s.SessionLabel)
            .Filter(filters, timeProvider)
            .OrderByDescending(a => a.Session.Date)
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
            .Include(a => a.Session)
                .ThenInclude(s => s.SessionLabel)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Used by the QR check-in flow to detect a prior check-in for the same
    /// person at the same session.
    /// </summary>
    public Task<bool> ExistsForSessionAndPerson(
        Guid congregationId,
        Guid sessionId,
        Guid personId,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .AnyAsync(
                a =>
                    a.CongregationId == congregationId
                    && a.SessionId == sessionId
                    && a.PersonId == personId,
                ct
            );

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

    /// <summary>
    /// Counts attendance filtered by the person's role at the time the
    /// attendance was recorded. Not the person's current role.
    /// </summary>
    public Task<int> CountByKindAtTime(
        Guid congregationId,
        PersonKind kind,
        AttendanceFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId && a.PersonKindAtTime == kind)
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
            .Where(a =>
                a.CongregationId == congregationId && a.PersonKindAtTime == PersonKind.Visitor
            )
            .Filter(filters, timeProvider)
            .Where(a =>
                !_dbSet
                    .FilterDeleted()
                    .Any(prev =>
                        prev.PersonId == a.PersonId
                        && prev.PersonKindAtTime == PersonKind.Visitor
                        && prev.Session.Date < a.Session.Date
                    )
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
        if (filters.SessionId is not null)
            query = query.Where(a => a.SessionId == filters.SessionId);

        if (filters.SessionLabelId is not null)
            query = query.Where(a => a.Session.SessionLabelId == filters.SessionLabelId);

        if (filters.PersonId is not null)
            query = query.Where(a => a.PersonId == filters.PersonId);

        if (filters.Kind is not null)
            query = query.Where(a => a.PersonKindAtTime == filters.Kind);

        var from = filters.From ?? DateUtils.GetFirstDayOfWeek(timeProvider);
        var to = filters.To ?? DateUtils.GetDateToday(timeProvider);

        query = query.Where(a => a.Session.Date >= from && a.Session.Date <= to);

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
                a.Session.Date < cursor.SessionDate
                || (a.Session.Date == cursor.SessionDate && a.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
