using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class TransactionCategoryRepository(AppDbContext context)
{
    private readonly DbSet<TransactionCategory> _dbSet = context.Set<TransactionCategory>();

    public async Task<List<TransactionCategory>> GetAll(Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(c => c.CongregationId == congregationId)
            .ToListAsync(ct);
    }

    public async Task<TransactionCategory?> GetById(
        Guid congregationId,
        int id,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(c => c.Id == id && c.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<TransactionCategory>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(tc => tc.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(TransactionCategory entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(TransactionCategory entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}
