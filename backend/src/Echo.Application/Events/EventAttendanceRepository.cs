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
                .ThenInclude(m => m.Person)
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
                .ThenInclude(m => m.Person)
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
                .ThenInclude(m => m.Person)
            .Include(e => e.Event)
            .OrderByDescending(e => e.CheckInTime)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<EventAttendance>> ListByEventId(
        Guid congregationId,
        Guid eventId,
        EventAttendanceFilters filters,
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
            .Filter(filters, context.Set<EventRegistration>().FilterDeleted())
            .Include(e => e.Member)
                .ThenInclude(m => m.Person)
            .Include(e => e.Event)
            .OrderByDescending(e => e.CheckInTime)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    /// <summary>
    /// The check-ins of these members at one event, by member. The page of registrations asks for the people on it,
    /// so a registration can say whether they came, without the page reading every check-in.
    /// </summary>
    public Task<Dictionary<Guid, EventCheckInRefDto>> GetForMembers(
        Guid eventId,
        IReadOnlyList<Guid> memberIds,
        CancellationToken ct
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.EventId == eventId && memberIds.Contains(a.MemberId))
            .ToDictionaryAsync(
                a => a.MemberId,
                a => new EventCheckInRefDto { Id = a.Id, CheckInTime = a.CheckInTime },
                ct
            );

    /// <summary>How many checked in at an event, and how many of them had not registered.</summary>
    public async Task<EventRosterSummaryDto> GetRosterSummary(
        Guid congregationId,
        Guid eventId,
        CancellationToken ct
    )
    {
        var registrations = context.Set<EventRegistration>().FilterDeleted();
        var checkIns = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(a => a.CongregationId == congregationId && a.EventId == eventId);

        var counts = await context
            .OneRow(congregationId)
            .Select(_ => new
            {
                CheckedIn = checkIns.Count(),
                WalkIns = checkIns.Count(a =>
                    !registrations.Any(r => r.EventId == a.EventId && r.MemberId == a.MemberId)
                ),
            })
            .FirstOrDefaultAsync(ct);

        var checkedIn = counts?.CheckedIn ?? 0;
        var walkIns = counts?.WalkIns ?? 0;
        return new EventRosterSummaryDto
        {
            CheckedIn = checkedIn,
            WalkIns = walkIns,
            CameRegistered = checkedIn - walkIns,
        };
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
    internal static IQueryable<EventAttendance> Filter(
        this IQueryable<EventAttendance> query,
        EventAttendanceFilters filters,
        IQueryable<EventRegistration> registrations
    )
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.Name.Trim())}%";
            query = query.Where(a => EF.Functions.ILike(a.Member.Person.Name, pattern));
        }

        if (filters.MemberIds is { Length: > 0 } ids)
            query = query.Where(a => ids.Contains(a.MemberId));

        // A walk-in is a check-in with no registration for the same event and member.
        return filters.Kind switch
        {
            CheckInKind.Registered => query.Where(a =>
                registrations.Any(r => r.EventId == a.EventId && r.MemberId == a.MemberId)
            ),
            CheckInKind.WalkIn => query.Where(a =>
                !registrations.Any(r => r.EventId == a.EventId && r.MemberId == a.MemberId)
            ),
            _ => query,
        };
    }

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
