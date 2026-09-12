using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

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
            .FilterSoftDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .OrderByDescending(e => e.CheckInTime)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<EventAttendance?> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
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
            .FilterSoftDeleted()
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
            .FilterSoftDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Where(e => e.EventId == eventId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .OrderByDescending(e => e.CheckInTime)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
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
