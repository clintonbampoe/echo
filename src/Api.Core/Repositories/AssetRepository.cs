using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Api.Core.Dtos.Core;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

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
            .ApplyDateFilters(queryParameters)
            .ApplySearchFilter(queryParameters)
            .Where(a => a.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(a => new AssetListResponseDto(
                a.Id,
                a.Category.Name,
                a.Name,
                a.Status,
                a.CurrentValue
            ))
            .ApplyPagination(paginationParameters)
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
