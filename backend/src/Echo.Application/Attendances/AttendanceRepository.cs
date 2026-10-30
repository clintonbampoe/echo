using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Domain.Members;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            .OrderByDescending(a => a.SessionDate)
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
    /// Used to detect a prior check-in for the same
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
    /// recorded, not their current role. The filter parameters are the same ones the list
    /// endpoint takes, so a summary and its corresponding list describe the same records.
    /// </summary>
    public Task<AttendanceStats> GetSummary(
        Guid congregationId,
        AttendanceFilters f,
        CancellationToken ct
    ) =>
        Scope(congregationId, f)
            .GroupBy(_ => 1)
            .Select(g => new AttendanceStats(
                g.Count(),
                g.Count(a => a.PersonKindAtTime == PersonKind.Member),
                g.Count(a => a.PersonKindAtTime == PersonKind.Visitor),
                g.Count(a => a.IsFirstVisit)
            ))
            .SingleOrDefaultAsync(ct)
            .ContinueWith(t => t.Result ?? new AttendanceStats(0, 0, 0, 0), ct);

    public async Task<List<AttendanceMonthlyDto>> GetMonthlyTotals(
        Guid congregationId,
        AttendanceMonthlyFilters filters,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    ) =>
        await Scope(
                congregationId,
                new AttendanceFilters
                {
                    SessionLabelId = filters.SessionLabelId,
                    From = from,
                    To = to,
                }
            )
            .MonthlyTotals()
            .ToListAsync(ct);

    public async Task<HashSet<Guid>> PersonIdsPresent(
        Guid congregationId,
        Guid sessionId,
        List<Guid> personIds,
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

        return [.. present];
    }

    public void CreateMany(IEnumerable<Attendance> entities) => _dbSet.AddRange(entities);

    public void Create(Attendance entity) => _dbSet.Add(entity);

    public Task<List<Attendance>> ListForSession(Guid sessionId, CancellationToken ct = default) =>
        _dbSet.Live().Where(a => a.SessionId == sessionId).ToListAsync(ct);

    /// <summary>
    /// A visitor's first visit is their earliest live attendance as a visitor. Recomputes the
    /// flag for the visitors among <paramref name="changed"/> after any write that could move it,
    /// touching only rows whose flag actually changes. Idempotent, so a failed or repeated call
    /// is repaired by the next one.
    /// </summary>
    public async Task RefreshFirstVisits(
        IEnumerable<Attendance> changed,
        CancellationToken ct = default
    )
    {
        var visitorIds = changed
            .Where(a => a.PersonKindAtTime == PersonKind.Visitor)
            .Select(a => a.PersonId)
            .Distinct()
            .ToArray();
            
        if (visitorIds.Length == 0)
            return;

        await context.Database.ExecuteSqlAsync(
            $@"
            WITH ranked AS (
                SELECT id,
                       is_first_visit AS current,
                       row_number() OVER (PARTITION BY person_id ORDER BY session_date, id) = 1 AS should_be
                FROM attendance
                WHERE person_id = ANY({visitorIds})
                  AND person_kind_at_time = 'Visitor'
                  AND deleted_at IS NULL
            )
            UPDATE attendance a
            SET is_first_visit = r.should_be
            FROM ranked r
            WHERE a.id = r.id AND r.current <> r.should_be",
            ct
        );
    }

    public void SoftDelete(Attendance entity) => entity.DeletedAt = DateTime.UtcNow;

    private IQueryable<Attendance> Scope(Guid congregationId, AttendanceFilters filters)
    {
        return context
            .Attendances.AsNoTracking()
            .Where(a => a.CongregationId == congregationId)
            .Live()
            .Filter(filters);
    }
}

public record AttendanceStats(int Total, int Members, int Visitors, int FirstTimeVisitors);

internal static class AttendanceQueryExtensions
{
    // Who was present at each of the sessions, split by what they were when they came.
    internal static IQueryable<SessionAttendance> AttendancePerSession(
        this IQueryable<Attendance> query,
        IReadOnlyList<Guid> sessionIds
    ) =>
        query
            .Live()
            .Where(a => sessionIds.Contains(a.SessionId))
            .GroupBy(a => a.SessionId)
            .Select(g => new SessionAttendance(
                g.Key,
                g.Count(),
                g.Count(a => a.PersonKindAtTime == PersonKind.Member),
                g.Count(a => a.PersonKindAtTime == PersonKind.Visitor),
                g.Count(a => a.IsFirstVisit)
            ));

    internal static IQueryable<AttendanceMonthlyDto> MonthlyTotals(
        this IQueryable<Attendance> query
    ) =>
        query
            .GroupBy(a => new { a.SessionDate.Year, a.SessionDate.Month })
            .Select(g => new AttendanceMonthlyDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalPresent = g.Count(),
                MembersPresent = g.Count(a => a.PersonKindAtTime == PersonKind.Member),
                VisitorsPresent = g.Count(a => a.PersonKindAtTime == PersonKind.Visitor),
                FirstTimeVisitors = g.Count(a => a.IsFirstVisit),
            });

    // Deleting a session soft-deletes its attendance, so a record's own DeletedAt decides visibility.
    internal static IQueryable<Attendance> Live(this IQueryable<Attendance> query) =>
        query.FilterDeleted();

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
            query = query.Where(a => a.SessionDate >= filters.From);

        if (filters.To is not null)
            query = query.Where(a => a.SessionDate <= filters.To);

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
        // The leading <= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every newer row.
        if (cursor is not null)
            query = query.Where(a =>
                a.SessionDate <= cursor.SessionDate
                && (a.SessionDate < cursor.SessionDate || a.Id < cursor.Id)
            );

        return query.Take(pageSize);
    }
}
