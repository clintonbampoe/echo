using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
