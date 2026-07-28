using Echo.Application.Extensions.QueryMethods;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AssetCategoryRepository(AppDbContext context)
    : ReferenceRepositoryBase<AssetCategory>(context)
{
    public async Task<List<AssetCategoryResponseDto>> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.CongregationId == congregationId)
            .Select(c => new AssetCategoryResponseDto { Id = c.Id, Name = c.Name })
            .ToListAsync(ct);
    }

    public async Task<AssetCategoryResponseDto?> GetByIdAsync(
        int id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.Id == id && c.CongregationId == congregationId)
            .Select(c => new AssetCategoryResponseDto { Id = c.Id, Name = c.Name })
            .FirstOrDefaultAsync(ct);
    }
}
