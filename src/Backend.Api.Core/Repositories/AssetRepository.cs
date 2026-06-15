using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class AssetRepository(AppDbContext context) : PrimaryRepositoryBase<Asset>(context)
{
    public async Task<PagedResponse<AssetListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(a => a.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .ApplyPagination(paginationParameters)
            .Select(a => new AssetListResponseDto(
                a.Id,
                a.Category.Name,
                a.Name,
                a.Status,
                a.CurrentValue
            ))
            .ToListAsync(ct);

        return new PagedResponse<AssetListResponseDto>(records, paginationParameters, totalRecords);
    }

    public async Task<AssetResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(a => a.Id == id)
            .Select(a => new AssetResponseDto(
                a.Id,
                a.CategoryId,
                a.Category.Name,
                a.Name,
                a.SerialNumber,
                a.PurchaseDate,
                a.PurchaseCost,
                a.CurrentValue,
                a.Status,
                a.Description,
                a.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
