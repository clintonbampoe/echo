using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

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
            .Select(a => new AssetListResponseDto
            {
                Id = a.Id,
                CategoryName =  a.Category.Name,
                Name =  a.Name,
                Status = a.Status,
                CurrentValue = a.CurrentValue
            })
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
            .Select(a => new AssetResponseDto
            {
                Id = a.Id,
                CategoryId = a.Category.Id,
                CategoryName = a.Category.Name,
                Name = a.Name,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchaseCost =  a.PurchaseCost,
                CurrentValue = a.CurrentValue,
                Status = a.Status,
                Description =  a.Description,
                CreatedAt =  a.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
