using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class EventRepository(AppDbContext context)
{
    private readonly DbSet<Event> _dbSet = context.Set<Event>();

    public async Task<List<Event>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        Parameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(e => e.CongregationId == congregationId);

        var res = await query
            .OrderBy(e => e.Id)
            .Include(e => e.Organization)
            .Include(e => e.Organizer)
            .ToListAsync(ct);

        return res;
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

    public Task GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
