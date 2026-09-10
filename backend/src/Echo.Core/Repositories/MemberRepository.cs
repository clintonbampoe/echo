using Echo.Application.Extensions.QueryExtensions;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class MemberRepository(AppDbContext context)
{
    private readonly DbSet<Member> _dbSet = context.Set<Member>();

    public async Task<List<Member>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(m => m.CongregationId == congregationId);

        var res = await query.OrderBy(m => m.Id).ToListAsync(ct);
        return res;
    }

    public async Task<Member?> GetById(Guid id, Guid congregationId, CancellationToken ct = default)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(m => m.Id == id && m.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Member>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(m => m.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(Member entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Member entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task GetSummary(Guid congregationId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
