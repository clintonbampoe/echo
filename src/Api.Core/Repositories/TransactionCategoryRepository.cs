using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

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
            .Select(c => new TransactionCategoryResponseDto(c.Id, c.Name, c.CategoryType))
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
            .Select(c => new TransactionCategoryResponseDto(c.Id, c.Name, c.CategoryType))
            .FirstOrDefaultAsync(ct);
    }
}
