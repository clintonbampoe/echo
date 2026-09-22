using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Transactions;

public class TransactionCategoryRepository(AppDbContext context)
{
    private readonly DbSet<TransactionCategory> _dbSet = context.Set<TransactionCategory>();

    public async Task<List<TransactionCategory>> GetAll(Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
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
            .FilterDeleted()
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
            .FilterDeleted()
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
