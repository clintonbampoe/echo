using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class EventRegistrationRepository(AppDbContext context)
{
    private readonly DbSet<EventRegistration> _dbSet = context.Set<EventRegistration>();

    public async Task<List<EventRegistration>> GetAll(
        Guid congregationId,
        PaginationParameters paginationParameters,
        Parameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.CongregationId == congregationId);

        var res = await query
            .OrderBy(e => e.Id)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .ToListAsync(ct);

        return res;
    }

    public async Task<EventRegistration?> GetById(
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

    public void Create(EventRegistration entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(EventRegistration entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public async Task<List<EventRegistration>> GetByMemberId(
        PaginationParameters paginationParameters,
        Parameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.MemberId == memberId);

        var res = await query
            .OrderBy(e => e.Event.StartDate)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .ToListAsync(ct);

        return res;
    }

    public async Task<List<EventRegistration>> GetByEventId(
        PaginationParameters paginationParameters,
        Parameters queryParameters,
        Guid eventId,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.EventId == eventId);

        var res = await query
            .OrderBy(e => e.Member.Name)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .ToListAsync(ct);

        return res;
    }
}
