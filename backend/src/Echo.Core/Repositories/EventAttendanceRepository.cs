using Echo.Application.Extensions.QueryExtensions;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class EventAttendanceRepository(AppDbContext context)
{
    private readonly DbSet<EventAttendance> _dbSet = context.Set<EventAttendance>();

    public async Task<List<EventAttendance>> GetAll(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.CongregationId == congregationId);

        var res = await query
            .OrderByDescending(e => e.CreatedAt)
            .Include(e => e.Member)
            .Include(e => e.Event)
            .ToListAsync(ct);

        return res;
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

    public async Task<List<EventAttendance>> GetByMemberId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
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

    public async Task<List<EventAttendance>> GetByEventId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
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
