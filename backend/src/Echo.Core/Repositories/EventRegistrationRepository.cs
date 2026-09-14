using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class EventRegistrationRepository(AppDbContext context)
{
    private readonly DbSet<EventRegistration> _dbSet = context.Set<EventRegistration>();

    public async Task<List<EventRegistration>> List(
        Guid congregationId,
        EventRegistrationCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .OrderByDescending(e => e.RegistrationDate)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<EventRegistration?> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(e => e.Id == id && e.CongregationId == congregationId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(EventRegistration entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(EventRegistration entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public async Task<List<EventRegistration>> ListByMemberId(
        Guid congregationId,
        Guid memberId,
        EventRegistrationCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Where(e => e.MemberId == memberId)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .OrderByDescending(e => e.RegistrationDate)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<EventRegistration>> ListByEventId(
        Guid congregationId,
        Guid eventId,
        EventRegistrationCursor? cursor,
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
            .OrderByDescending(e => e.RegistrationDate)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }
}

internal static class EventRegistrationQueryExtensions
{
    internal static IQueryable<EventRegistration> Paginate(
        this IQueryable<EventRegistration> query,
        EventRegistrationCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(e =>
                e.RegistrationDate < cursor.RegistrationDate
                || (e.RegistrationDate == cursor.RegistrationDate && e.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
