using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class TransactionCategoryRepository(AppDbContext context)
    : ReferenceRepositoryBase<TransactionCategory>(context)
{
    public async Task<List<TransactionCategoryResponseDto>> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.CongregationId == congregationId)
            .Select(c => new TransactionCategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                CategoryType = c.CategoryType
            })
            .ToListAsync(ct);
    }

    public async Task<TransactionCategoryResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.Id == id)
            .Select(c => new TransactionCategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                CategoryType = c.CategoryType
            })
            .FirstOrDefaultAsync(ct);
    }
}
