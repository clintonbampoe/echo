using Echo.Application.Extensions.QueryExtensions;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AssetRepository(AppDbContext context)
{
    private readonly DbSet<Asset> _dbSet = context.Set<Asset>();

    public async Task<List<Asset>> GetPage(
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
            .ApplySearchFilter(queryParameters)
            .Where(a => a.CongregationId == congregationId);

        var res = await query.OrderBy(a => a.Id).Include(a => a.Category).ToListAsync(ct);
        return res;
    }

    public async Task<Asset?> GetById(Guid id, Guid congregationId, CancellationToken ct = default)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(a => a.Id == id && a.CongregationId == congregationId)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Asset>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(a => a.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(Asset entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Asset entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
