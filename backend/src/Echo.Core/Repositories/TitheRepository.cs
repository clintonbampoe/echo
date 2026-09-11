using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class TitheRepository(AppDbContext context)
{
    private readonly DbSet<Tithe> _dbSet = context.Set<Tithe>();

    public async Task<List<Tithe>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        Parameters? queryParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(t => t.CongregationId == congregationId);

        var res = await query
            .OrderBy(t => t.ForYear)
            .ThenBy(t => t.ForMonth)
            .Include(t => t.Member)
            .ToListAsync(ct);

        return res;
    }

    public async Task<Tithe?> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Include(t => t.Member)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(Tithe entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Tithe entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task GetSummary(Guid congregationId, int year, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
