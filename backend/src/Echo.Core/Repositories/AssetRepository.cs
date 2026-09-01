using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;
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
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .ApplySearchFilter(queryParameters)
            .Where(a => a.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(a => new AssetListResponseDto
            {
                Id = a.Id,
                CategoryName = a.Category.Name,
                Name = a.Name,
                Status = a.Status,
                CurrentValue = a.CurrentValue,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<AssetListResponseDto>(records, paginationParameters, totalRecords);
    }

    public async Task<AssetResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.Id == id && a.CongregationId == congregationId)
            .Select(a => new AssetResponseDto
            {
                Id = a.Id,
                CategoryId = a.Category.Id,
                CategoryName = a.Category.Name,
                Name = a.Name,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchaseCost = a.PurchaseCost,
                CurrentValue = a.CurrentValue,
                Status = a.Status,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AssetSummaryDto> GetSummaryAsync(Guid congregationId, CancellationToken ct = default)
    {
        var stats = await DbSet
            .Where(a => a.CongregationId == congregationId && a.Status != AssetStatus.Liquidated)
            .GroupBy(a => 1)
            .Select(g => new
            {
                TotalAssets = g.Count(),
                TotalCurrentValue = g.Sum(a => a.CurrentValue),
                UnderMaintenance = g.Count(a => a.Status == AssetStatus.UnderMaintenance),
                TotalDepreciation = g.Sum(a => a.PurchaseCost - a.CurrentValue),
            })
            .FirstOrDefaultAsync(ct);

        return new AssetSummaryDto
        {
            TotalAssets = stats?.TotalAssets ?? 0,
            TotalCurrentValue = stats?.TotalCurrentValue ?? 0,
            UnderMaintenance = stats?.UnderMaintenance ?? 0,
            TotalDepreciation = stats?.TotalDepreciation ?? 0,
        };
    }
}
