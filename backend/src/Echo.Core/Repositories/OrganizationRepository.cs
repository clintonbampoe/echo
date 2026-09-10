using Echo.Application.Extensions.QueryExtensions;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class OrganizationRepository(AppDbContext context)
{
    private readonly DbSet<Organization> _dbSet = context.Set<Organization>();

    public async Task<List<Organization>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .ApplySearchFilter(queryParameters)
            .Where(o => o.CongregationId == congregationId);

        var res = await query.OrderBy(o => o.Id).ToListAsync(ct);

        return res;
    }

    public async Task<Organization?> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(o => o.Id == id && o.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Organization>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(o => o.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(Organization entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Organization entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
