using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class TransactionRepository(AppDbContext context)
{
    private readonly DbSet<Transaction> _dbSet = context.Set<Transaction>();

    public async Task<List<Transaction>> GetPage(
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
            .OrderByDescending(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .Include(t => t.Category)
            .ToListAsync(ct);

        return res;
    }

    public async Task<Transaction?> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(Transaction entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Transaction entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
