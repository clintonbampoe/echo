using Echo.Data;
using Echo.Domain.Events;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Echo.Domain.Congregations;

namespace Echo.Application.Events;

public class EventRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<Event> _dbSet = context.Set<Event>();
    private readonly DbSet<EventRegistration> _registrationDbSet = context.Set<EventRegistration>();
    private readonly DbSet<EventAttendance> _attendanceDbSet = context.Set<EventAttendance>();

    public async Task<List<Event>> List(
        Guid congregationId,
        EventFilters filters,
        EventCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Include(e => e.Organization)
            .Include(e => e.Organizer)
                .ThenInclude(o => o.Person)
            .Filter(filters)
            .OrderBy(e => e.StartDate)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Event?> GetById(Guid congregationId, Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(e => e.Id == id && e.CongregationId == congregationId)
            .Include(e => e.Organizer)
                .ThenInclude(o => o.Person)
            .Include(e => e.Organization)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Event>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, int>> GetRegistrationCounts(
        Guid congregationId,
        IReadOnlyList<Guid> eventIds,
        CancellationToken ct = default
    )
    {
        if (eventIds.Count == 0)
            return [];

        return await context
            .Set<EventRegistration>()
            .AsNoTracking()
            .Where(r => r.CongregationId == congregationId)
            .CountPerEvent(eventIds)
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
    }

    public async Task<EventStats> GetSummaryStats(
        Guid congregationId,
        EventFilters filters,
        CancellationToken ct = default
    )
    {
        var events = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Filter(filters);

        var today = DateUtils.GetDateToday(timeProvider);
        var upcoming = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Upcoming(filters, today);

        var stats = await context
            .OneRow(congregationId)
            .SummarizeEvents(
                events,
                upcoming,
                _registrationDbSet.FilterDeleted(),
                _attendanceDbSet.FilterDeleted()
            )
            .FirstOrDefaultAsync(ct);
        return stats ?? new EventStats(0, 0, 0, 0);
    }

    public void Create(Event entity) => _dbSet.Add(entity);

    public void SoftDelete(Event entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record EventStats(int Total, int Upcoming, int Registrations, int Attendees);

internal static class EventQueryExtensions
{
    internal static IQueryable<ParentCount> CountPerEvent(
        this IQueryable<EventRegistration> query,
        IReadOnlyList<Guid> eventIds
    ) =>
        query
            .FilterDeleted()
            .Where(r => eventIds.Contains(r.EventId))
            .GroupBy(r => r.EventId)
            .Select(g => new ParentCount(g.Key, g.Count()));

    // Upcoming means "starts today or later", whatever date window the list is showing. The other
    // filters (name, organization, organizer) still apply.
    internal static IQueryable<Event> Upcoming(
        this IQueryable<Event> query,
        EventFilters filters,
        DateOnly today
    ) => query.Filter(filters with { From = null, To = null }).Where(e => e.StartDate >= today);

    internal static IQueryable<EventStats> SummarizeEvents(
        this IQueryable<Congregation> oneRow,
        IQueryable<Event> events,
        IQueryable<Event> upcoming,
        IQueryable<EventRegistration> registrations,
        IQueryable<EventAttendance> attendance
    ) =>
        oneRow.Select(_ => new EventStats(
            events.Count(),
            upcoming.Count(),
            events.SelectMany(e => registrations.Where(r => r.EventId == e.Id)).Count(),
            events.SelectMany(e => attendance.Where(a => a.EventId == e.Id)).Count()
        ));

    internal static IQueryable<Event> Filter(
        this IQueryable<Event> query,
        EventFilters filters
    )
    {
        if (filters.Name is not null)
            query = query.Where(e => EF.Functions.ILike(e.Name, $"%{filters.Name}%"));

        if (filters.OrganizerId is not null)
            query = query.Where(e => e.OrganizerId == filters.OrganizerId);

        if (filters.OrganizationId is not null)
            query = query.Where(e => e.OrganizationId == filters.OrganizationId);

        if (filters.From is not null)
            query = query.Where(e => e.StartDate >= filters.From);

        if (filters.To is not null)
            query = query.Where(e => e.StartDate <= filters.To);

        return query;
    }

    internal static IQueryable<Event> Paginate(
        this IQueryable<Event> query,
        EventCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(e =>
                e.StartDate > cursor.StartDate
                || (e.StartDate == cursor.StartDate && e.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
