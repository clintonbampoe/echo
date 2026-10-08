using Echo.Data;
using Echo.Domain.Events;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Events;

public class EventAttendanceRepository(AppDbContext context)
{
    private readonly DbSet<EventAttendance> _dbSet = context.Set<EventAttendance>();

    public async Task<List<EventAttendance>> List(
        Guid congregationId,
        EventAttendanceCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .OrderByDescending(e => e.CheckInTime)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<EventAttendance?> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(e => e.Id == id && e.CongregationId == congregationId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(EventAttendance entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(EventAttendance entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public async Task<List<EventAttendance>> ListByMemberId(
        Guid congregationId,
        Guid memberId,
        EventAttendanceCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Where(e => e.MemberId == memberId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .OrderByDescending(e => e.CheckInTime)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<EventAttendance>> ListByEventId(
        Guid congregationId,
        Guid eventId,
        EventAttendanceCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Where(e => e.EventId == eventId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .OrderByDescending(e => e.CheckInTime)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsForEventAndMember(
        Guid congregationId,
        Guid eventId,
        Guid memberId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .AnyAsync(
                e =>
                    e.CongregationId == congregationId
                    && e.EventId == eventId
                    && e.MemberId == memberId,
                ct
            );
    }
}

internal static class EventAttendanceQueryExtensions
{
    internal static IQueryable<EventAttendance> Paginate(
        this IQueryable<EventAttendance> query,
        EventAttendanceCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(e =>
                e.CheckInTime < cursor.CheckInTime
                || (e.CheckInTime == cursor.CheckInTime && e.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
