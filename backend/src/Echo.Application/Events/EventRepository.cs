using Echo.Data;
using Echo.Domain.Events;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Echo.Domain.Congregations;
using Echo.Shared.Pagination;

namespace Echo.Application.Events;

public class EventRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<Event> _dbSet = context.Set<Event>();
    private readonly DbSet<EventRegistration> _registrationDbSet = context.Set<EventRegistration>();
    private readonly DbSet<EventAttendance> _attendanceDbSet = context.Set<EventAttendance>();

    public async Task<List<Event>> List(
        Guid congregationId,
        EventListFilters filters,
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
            .OrderForList(filters.Order)
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

    // The event ids come from a query already limited to the congregation, so the rows are not
    // limited again: a congregation filter would send the database to the table for every row
    // instead of counting from the (event, member) index.
    public async Task<Dictionary<Guid, int>> GetRegistrationCounts(
        IReadOnlyList<Guid> eventIds,
        CancellationToken ct = default
    )
    {
        if (eventIds.Count == 0)
            return [];

        return await _registrationDbSet
            .AsNoTracking()
            .CountPerEvent(eventIds)
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
    }

    public async Task<Dictionary<Guid, int>> GetAttendanceCounts(
        IReadOnlyList<Guid> eventIds,
        CancellationToken ct = default
    )
    {
        if (eventIds.Count == 0)
            return [];

        return await _attendanceDbSet
            .AsNoTracking()
            .CountPerEvent(eventIds)
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
    }

    // The newest events in the filters, each with how many registered and how many came.
    public async Task<List<EventSeriesDto>> GetSeries(
        Guid congregationId,
        EventFilters filters,
        int limit,
        CancellationToken ct = default
    ) =>
        await Scope(congregationId, filters)
            .Series(_registrationDbSet.FilterDeleted(), _attendanceDbSet.FilterDeleted(), limit)
            .ToListAsync(ct);

    public async Task<List<EventMonthRow>> GetMonthlyTotals(
        Guid congregationId,
        EventMonthlyFilters filters,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    ) =>
        await Scope(
                congregationId,
                new EventFilters
                {
                    OrganizationId = filters.OrganizationId,
                    OrganizerId = filters.OrganizerId,
                    From = from,
                    To = to,
                }
            )
            .MonthlyTotals(_registrationDbSet.FilterDeleted(), _attendanceDbSet.FilterDeleted())
            .ToListAsync(ct);

    private IQueryable<Event> Scope(Guid congregationId, EventFilters filters) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Filter(filters);

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

public record EventMonthRow(int Year, int Month, int Events, int Registrations, int Attendance);

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

    internal static IQueryable<ParentCount> CountPerEvent(
        this IQueryable<EventAttendance> query,
        IReadOnlyList<Guid> eventIds
    ) =>
        query
            .FilterDeleted()
            .Where(a => eventIds.Contains(a.EventId))
            .GroupBy(a => a.EventId)
            .Select(g => new ParentCount(g.Key, g.Count()));

    // The newest events first. Each count is a count from the (event, member) index.
    internal static IQueryable<EventSeriesDto> Series(
        this IQueryable<Event> events,
        IQueryable<EventRegistration> registrations,
        IQueryable<EventAttendance> attendance,
        int limit
    ) =>
        events
            .OrderByDescending(e => e.StartDate)
            .ThenByDescending(e => e.Id)
            .Take(limit)
            .Select(e => new EventSeriesDto
            {
                EventId = e.Id,
                Name = e.Name,
                StartDate = e.StartDate,
                Capacity = e.Capacity,
                RegistrationCount = registrations.Count(r => r.EventId == e.Id),
                AttendanceCount = attendance.Count(a => a.EventId == e.Id),
            });

    // The events grouped by the month they start in; each event's counts are taken first, so the
    // groups only add them up.
    internal static IQueryable<EventMonthRow> MonthlyTotals(
        this IQueryable<Event> events,
        IQueryable<EventRegistration> registrations,
        IQueryable<EventAttendance> attendance
    ) =>
        events
            .Select(e => new
            {
                e.StartDate.Year,
                e.StartDate.Month,
                Registered = registrations.Count(r => r.EventId == e.Id),
                CameIn = attendance.Count(a => a.EventId == e.Id),
            })
            .GroupBy(x => new { x.Year, x.Month })
            .Select(g => new EventMonthRow(
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Sum(x => x.Registered),
                g.Sum(x => x.CameIn)
            ));

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
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.Name.Trim())}%";
            query = query.Where(e => EF.Functions.ILike(e.Name, pattern));
        }

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

    internal static IOrderedQueryable<Event> OrderForList(
        this IQueryable<Event> query,
        SortDirection order
    ) =>
        order == SortDirection.Desc
            ? query.OrderByDescending(e => e.StartDate).ThenByDescending(e => e.Id)
            : query.OrderBy(e => e.StartDate).ThenBy(e => e.Id);

    // The cursor is the last row of the previous page and says which direction it was listed in. The
    // leading date bound is redundant but gives the index a bound to start from; without it the OR
    // is only a filter and a deep page walks every row before it.
    internal static IQueryable<Event> Paginate(
        this IQueryable<Event> query,
        EventCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query =
                cursor.Order == SortDirection.Desc
                    ? query.Where(e =>
                        e.StartDate <= cursor.StartDate
                        && (e.StartDate < cursor.StartDate || e.Id < cursor.Id)
                    )
                    : query.Where(e =>
                        e.StartDate >= cursor.StartDate
                        && (e.StartDate > cursor.StartDate || e.Id > cursor.Id)
                    );

        return query.Take(pageSize);
    }
}
