using Echo.Data;
using Echo.Domain.Events;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

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

    public Task<int> Count(
        Guid congregationId,
        EventFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Filter(filters)
            .CountAsync(ct);

    public Task<int> CountUpcoming(
        Guid congregationId,
        EventFilters filters,
        CancellationToken ct = default
    )
    {
        var today = DateUtils.GetDateToday(timeProvider);
        return _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId && e.StartDate >= today)
            .Filter(filters)
            .CountAsync(ct);
    }

    public Task<int> CountRegistrations(
        Guid congregationId,
        EventFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Filter(filters)
            .SelectMany(e => _registrationDbSet.FilterDeleted().Where(r => r.EventId == e.Id))
            .CountAsync(ct);

    public Task<int> CountAttendees(
        Guid congregationId,
        EventFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Filter(filters)
            .SelectMany(e => _attendanceDbSet.FilterDeleted().Where(a => a.EventId == e.Id))
            .CountAsync(ct);

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

    public void Create(Event entity) => _dbSet.Add(entity);

    public void SoftDelete(Event entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class EventQueryExtensions
{
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
