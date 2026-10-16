using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Attendances;

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
            .Live()
            .Where(a => a.CongregationId == congregationId)
            .Include(a => a.Person)
            .Include(a => a.Session)
                .ThenInclude(s => s.SessionLabel)
            .Filter(filters)
            .OrderByDescending(a => a.Session.Date)
            .ThenByDescending(a => a.Id)
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
            .Live()
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
            .Live()
            .AnyAsync(
                a =>
                    a.CongregationId == congregationId
                    && a.SessionId == sessionId
                    && a.PersonId == personId,
                ct
            );

    /// <summary>
    /// Members and visitors are counted by the person's role at the time the attendance was
    /// recorded, not their current role.
    /// </summary>
    public async Task<AttendanceStats> GetSummaryStats(
        Guid congregationId,
        AttendanceFilters filters,
        CancellationToken ct = default
    )
    {
        var records = _dbSet
            .AsNoTracking()
            .Live()
            .Where(a => a.CongregationId == congregationId)
            .Filter(filters);

        var stats = await context
            .OneRow(congregationId)
            .SummarizeAttendance(records, _dbSet.Live())
            .FirstOrDefaultAsync(ct);
        return stats ?? new AttendanceStats(0, 0, 0, 0);
    }

    public async Task<HashSet<Guid>> PersonIdsPresent(
        Guid congregationId,
        Guid sessionId,
        IReadOnlyList<Guid> personIds,
        CancellationToken ct = default
    )
    {
        var present = await _dbSet
            .AsNoTracking()
            .Live()
            .Where(a =>
                a.CongregationId == congregationId
                && a.SessionId == sessionId
                && personIds.Contains(a.PersonId)
            )
            .Select(a => a.PersonId)
            .ToListAsync(ct);

        return present.ToHashSet();
    }

    public void CreateMany(IEnumerable<Attendance> entities) => _dbSet.AddRange(entities);

    public void Create(Attendance entity) => _dbSet.Add(entity);

    public void SoftDelete(Attendance entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record AttendanceStats(int Total, int Members, int Visitors, int FirstTimeVisitors);

internal static class AttendanceQueryExtensions
{
    // allLive is every live attendance record, not just the ones in scope: a visitor is a first-time
    // visitor when they have no earlier visit at all.
    internal static IQueryable<AttendanceStats> SummarizeAttendance(
        this IQueryable<Congregation> oneRow,
        IQueryable<Attendance> records,
        IQueryable<Attendance> allLive
    ) =>
        oneRow.Select(_ => new AttendanceStats(
            records.Count(),
            records.Count(a => a.PersonKindAtTime == PersonKind.Member),
            records.Count(a => a.PersonKindAtTime == PersonKind.Visitor),
            records
                .Where(a =>
                    a.PersonKindAtTime == PersonKind.Visitor
                    && !allLive.Any(prev =>
                        prev.PersonId == a.PersonId
                        && prev.PersonKindAtTime == PersonKind.Visitor
                        && prev.Session.Date < a.Session.Date
                    )
                )
                .Select(a => a.PersonId)
                .Distinct()
                .Count()
        ));

    // An attendance record is visible only while both it and its session are not deleted.
    internal static IQueryable<Attendance> Live(this IQueryable<Attendance> query) =>
        query.FilterDeleted().Where(a => a.Session.DeletedAt == null);

    internal static IQueryable<Attendance> InSessions(
        this IQueryable<Attendance> query,
        IQueryable<Session> sessions
    ) => query.Where(a => sessions.Any(s => s.Id == a.SessionId));

    internal static IQueryable<Attendance> Filter(
        this IQueryable<Attendance> query,
        AttendanceFilters filters
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

        if (filters.From is not null)
            query = query.Where(a => a.Session.Date >= filters.From);

        if (filters.To is not null)
            query = query.Where(a => a.Session.Date <= filters.To);

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var pattern = $"%{Shared.Query.Extensions.EscapeLike(filters.Name.Trim())}%";
            query = query.Where(a => EF.Functions.ILike(a.Person.Name, pattern));
        }

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
                || (a.Session.Date == cursor.SessionDate && a.Id < cursor.Id)
            );

        return query.Take(pageSize);
    }
}
