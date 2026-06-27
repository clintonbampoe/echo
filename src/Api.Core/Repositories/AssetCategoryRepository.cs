using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class AssetCategoryRepository(AppDbContext context)
    : ReferenceRepositoryBase<AssetCategory>(context)
{
    public async Task<List<AssetCategoryResponseDto>> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.CongregationId == congregationId)
            .Select(c => new AssetCategoryResponseDto(c.Id, c.Name))
            .ToListAsync(ct);
    }

    public async Task<AssetCategoryResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.Id == id)
            .Select(c => new AssetCategoryResponseDto(c.Id, c.Name))
            .FirstOrDefaultAsync(ct);
    }
}
