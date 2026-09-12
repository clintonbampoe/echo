using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class EventRepository(AppDbContext context)
{
    private readonly DbSet<Event> _dbSet = context.Set<Event>();

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
            .FilterSoftDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Include(e => e.Organization)
            .Include(e => e.Organizer)
            .Filter(filters)
            .OrderBy(e => e.StartDate)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Event?> GetById(Guid id, Guid congregationId, CancellationToken ct = default)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(e => e.Id == id && e.CongregationId == congregationId)
            .Include(e => e.Organizer)
            .Include(e => e.Organization)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(Event entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Event entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public async Task<List<Event>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(a => a.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }
}

internal static class EventQueryExtensions
{
    internal static IQueryable<Event> Filter(this IQueryable<Event> query, EventFilters filters)
    {
        var dateToday = DateOnly.FromDateTime(TimeProvider.System.GetUtcNow().DateTime);

        query = filters.StartDate is not null
            ? query.Where(e => e.StartDate == filters.StartDate)
            : query.Where(e => e.StartDate > dateToday);

        if (filters.OrganizerId is not null)
            query = query.Where(e => e.OrganizerId == filters.OrganizerId);

        if (filters.OrganizationId is not null)
            query = query.Where(e => e.OrganizationId == filters.OrganizationId);

        if (filters.Name is not null)
            query = query.Where(e => EF.Functions.ILike(e.Name, $"%{filters.Name}%"));

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
