using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Domain.Congregations;

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
        // No congregation filter on attendance: the sessions are already the congregation's, and asking for it here
        // sends the database through every attendance row of the congregation instead of walking the sessions.
        var attendance = context.Set<Attendance>().AsNoTracking().Live();

        var stats = await context
            .OneRow(congregationId)
            .SummarizeSessions(sessions)
            .SingleOrDefaultAsync(ct);
        if (stats is null)
            return new SessionStats(0, 0, 0, 0, 0);

        // Attendance is counted by walking the sessions in range and their rows in the (session, person) index,
        // one pass for both counts. Asking "is this row in one of those sessions?" for every attendance row sends
        // the database through the whole table, and a year of a big congregation is hundreds of thousands of rows.
        var records = await sessions
            .Join(attendance, s => s.Id, a => a.SessionId, (s, _) => s.Status)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                All = g.Count(),
                Completed = g.Count(status => status == SessionStatus.Completed),
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

public record SessionAttendance(
    Guid Id,
    int Total,
    int Members,
    int Visitors,
    int FirstTime
);

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
