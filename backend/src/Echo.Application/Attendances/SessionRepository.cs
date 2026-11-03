using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Attendances;

public class SessionRepository(AppDbContext context)
{
    private readonly DbSet<Session> _dbSet = context.Set<Session>();

    public async Task<List<Session>> List(
        Guid congregationId,
        SessionFilters filters,
        SessionCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(s => s.CongregationId == congregationId)
            .Include(s => s.SessionLabel)
            .Include(s => s.Creator)
            .Filter(filters)
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Session?> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(s => s.Id == id && s.CongregationId == congregationId)
            .Include(s => s.SessionLabel)
            .Include(s => s.Creator)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Returns who was present at each of the given sessions, split into members, visitors and
    /// first-time visitors, in a single query. Used by the list and detail endpoints without N+1.
    /// <br/><br/>
    /// Sessions with no attendance rows are absent from the dictionary; a missing entry means zero.
    /// The session ids come from a query already limited to the congregation, so the attendance is
    /// not limited again: a congregation filter would send the database to the table for every row
    /// instead of counting from the (session, person) index.
    /// </summary>
    public Task<Dictionary<Guid, SessionAttendance>> GetAttendance(
        IReadOnlyList<Guid> sessionIds,
        CancellationToken ct = default
    )
    {
        if (sessionIds.Count == 0)
            return Task.FromResult(new Dictionary<Guid, SessionAttendance>());

        return context
            .Set<Attendance>()
            .AsNoTracking()
            .AttendancePerSession(sessionIds)
            .ToDictionaryAsync(x => x.Id, ct);
    }

    /// <summary>
    /// The totals for a page of sessions. A session from before yesterday no longer changes, so its totals are read from
    /// the per-session view (one row each) instead of counting every check-in: the same figures the summaries use, up to
    /// the refresh interval behind. Recent sessions, and any session the view does not hold yet (a backdated one, or one
    /// nobody attended), are counted from the check-ins, so people marked present a moment ago show.
    /// </summary>
    public async Task<Dictionary<Guid, SessionAttendance>> GetAttendanceForList(
        IReadOnlyList<Session> sessions,
        DateOnly today,
        CancellationToken ct = default
    )
    {
        var settled = SettledIds(sessions, today);
        var totals =
            settled.Count == 0
                ? []
                : await context
                    .Set<SessionAttendanceTotals>()
                    .AsNoTracking()
                    .SettledAttendance(settled)
                    .ToDictionaryAsync(x => x.Id, ct);

        var counted = sessions.Select(s => s.Id).Where(id => !totals.ContainsKey(id)).ToList();
        foreach (var (id, attendance) in await GetAttendance(counted, ct))
            totals[id] = attendance;
        return totals;
    }

    // Yesterday's sessions may still be running past midnight in the congregation's time zone, so a day is left alone.
    internal static List<Guid> SettledIds(IEnumerable<Session> sessions, DateOnly today) =>
        [.. sessions.Where(s => s.Date < today.AddDays(-1)).Select(s => s.Id)];

    /// <summary>How many sessions were held (completed) among the ones the filters pick.</summary>
    public Task<int> CountCompleted(
        Guid congregationId,
        SessionFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(s => s.CongregationId == congregationId && s.Status == SessionStatus.Completed)
            .Filter(filters)
            .CountAsync(ct);

    public async Task<SessionStats> GetSummaryStats(
        Guid congregationId,
        SessionFilters filters,
        CancellationToken ct = default
    )
    {
        var sessions = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(s => s.CongregationId == congregationId)
            .Filter(filters);
        // The per-session totals hold one row per session, so a year is a few dozen rows to add up
        // instead of hundreds of thousands of check-ins. They are refreshed every few minutes.
        var totals = context.Set<SessionAttendanceTotals>().AsNoTracking();

        var stats = await context
            .OneRow(congregationId)
            .SummarizeSessions(sessions)
            .SingleOrDefaultAsync(ct);
        if (stats is null)
            return new SessionStats(0, 0, 0, 0, 0);

        var records = await sessions
            .Join(totals, s => s.Id, t => t.SessionId, (s, t) => new { s.Status, t.Total })
            .GroupBy(_ => 1)
            .Select(g => new
            {
                All = g.Sum(x => x.Total),
                Completed = g.Where(x => x.Status == SessionStatus.Completed).Sum(x => x.Total),
            })
            .FirstOrDefaultAsync(ct);

        return stats with
        {
            TotalAttendance = records?.All ?? 0,
            CompletedAttendance = records?.Completed ?? 0,
        };
    }

    public void Create(Session entity) => _dbSet.Add(entity);

    public void SoftDelete(Session entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record SessionAttendance(Guid Id, int Total, int Members, int Visitors, int FirstTime);

public record SessionStats(
    int Total,
    int Completed,
    int Cancelled,
    int TotalAttendance,
    int CompletedAttendance
);

internal static class SessionQueryExtensions
{
    // The session counts only; the attendance counts are filled in by the caller (see GetSummaryStats).
    internal static IQueryable<SessionStats> SummarizeSessions(
        this IQueryable<Congregation> oneRow,
        IQueryable<Session> sessions
    ) =>
        oneRow.Select(_ => new SessionStats(
            sessions.Count(),
            sessions.Count(s => s.Status == SessionStatus.Completed),
            sessions.Count(s => s.Status == SessionStatus.Cancelled),
            0,
            0
        ));

    internal static IQueryable<Session> Filter(
        this IQueryable<Session> query,
        SessionFilters filters
    )
    {
        if (filters.SessionLabelId is not null)
            query = query.Where(s => s.SessionLabelId == filters.SessionLabelId);

        if (filters.Status is not null)
            query = query.Where(s => s.Status == filters.Status);

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.Name.Trim())}%";
            query = query.Where(s => s.Name != null && EF.Functions.ILike(s.Name, pattern));
        }

        if (filters.From is not null)
            query = query.Where(s => s.Date >= filters.From);

        if (filters.To is not null)
            query = query.Where(s => s.Date <= filters.To);

        return query;
    }

    internal static IQueryable<Session> Paginate(
        this IQueryable<Session> query,
        SessionCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(s =>
                s.Date < cursor.Date || (s.Date == cursor.Date && s.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
