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
    /// Returns the attendance count for each of the given sessions in a
    /// single query. Used by list and detail endpoints to populate
    /// SessionResponseDto.AttendanceCount without N+1.
    /// <br/><br/>
    /// Sessions with no attendance rows are absent from the dictionary.
    /// Callers should use GetValueOrDefault(sessionId) — a missing entry
    /// means zero present.
    /// </summary>
    public Task<Dictionary<Guid, int>> GetAttendanceCounts(
        Guid congregationId,
        IReadOnlyList<Guid> sessionIds,
        CancellationToken ct = default
    )
    {
        if (sessionIds.Count == 0)
            return Task.FromResult(new Dictionary<Guid, int>());

        return context
            .Set<Attendance>()
            .AsNoTracking()
            .Live()
            .Where(a => a.CongregationId == congregationId && sessionIds.Contains(a.SessionId))
            .GroupBy(a => a.SessionId)
            .Select(g => new { SessionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SessionId, x => x.Count, ct);
    }

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
        var attendance = context
            .Set<Attendance>()
            .AsNoTracking()
            .Live()
            .Where(a => a.CongregationId == congregationId);

        var stats = await context
            .OneRow(congregationId)
            .SummarizeSessions(sessions, attendance)
            .FirstOrDefaultAsync(ct);
        return stats ?? new SessionStats(0, 0, 0, 0, 0);
    }

    public void Create(Session entity) => _dbSet.Add(entity);

    public void SoftDelete(Session entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class SessionQueryExtensions
{
    internal static IQueryable<SessionStats> SummarizeSessions(
        this IQueryable<Congregation> oneRow,
        IQueryable<Session> sessions,
        IQueryable<Attendance> attendance
    )
    {
        // Built before the projection: EF inlines these queries but cannot translate a custom
        // method call (InSessions) made inside the lambda.
        var completed = sessions.Where(s => s.Status == SessionStatus.Completed);
        var allRecords = attendance.InSessions(sessions);
        var completedRecords = attendance.InSessions(completed);

        return oneRow.Select(_ => new SessionStats(
            sessions.Count(),
            completed.Count(),
            sessions.Count(s => s.Status == SessionStatus.Cancelled),
            allRecords.Count(),
            completedRecords.Count()
        ));
    }

    internal static IQueryable<Session> Filter(
        this IQueryable<Session> query,
        SessionFilters filters
    )
    {
        if (filters.SessionLabelId is not null)
            query = query.Where(s => s.SessionLabelId == filters.SessionLabelId);

        if (filters.Status is not null)
            query = query.Where(s => s.Status == filters.Status);

        if (filters.Name is not null)
            query = query.Where(s =>
                s.Name != null && EF.Functions.ILike(s.Name, $"%{filters.Name}%")
            );

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
