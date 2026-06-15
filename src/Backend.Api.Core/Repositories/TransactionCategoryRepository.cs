using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
