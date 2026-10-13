using Echo.Data;
using Echo.Domain.Events;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Events;

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
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Include(e => e.Member)
                .ThenInclude(m => m.Person)
            .Include(e => e.Event)
            .OrderByDescending(e => e.RegistrationDate)
            .ThenBy(e => e.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<EventRegistration?> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct
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

    /// <summary>
    /// Registers the member unless the event already has `capacity` live registrations. The event row
    /// is locked for the duration, so two people going for the last seat are handled one after the
    /// other and only the first gets it.
    /// </summary>
    public async Task<bool> TryCreateWithinCapacity(
        EventRegistration entity,
        int capacity,
        CancellationToken ct = default
    )
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM \"Events\" WHERE \"Id\" = {entity.EventId} FOR UPDATE",
            ct
        );

        var registered = await _dbSet
            .FilterDeleted()
            .CountAsync(r => r.EventId == entity.EventId, ct);
        if (EventCapacity.IsFull(capacity, registered))
            return false;

        // Saved directly, inside the transaction that holds the lock. Nothing else is pending in this
        // request, so only the new registration is written.
        _dbSet.Add(entity);
        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
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
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Where(e => e.MemberId == memberId)
            .Include(e => e.Member)
                .ThenInclude(m => m.Person)
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
            .FilterDeleted()
            .Where(e => e.CongregationId == congregationId)
            .Where(e => e.EventId == eventId)
            .Include(e => e.Member)
                .ThenInclude(m => m.Person)
            .Include(e => e.Event)
            .OrderByDescending(e => e.RegistrationDate)
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
