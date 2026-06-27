using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class TitheRepository(AppDbContext context) : PrimaryRepositoryBase<Tithe>(context)
{
    public async Task<PagedResponse<TitheListResponseDto>> GetPageAsync(
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
            .Where(t => t.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(t => t.Id)
            .Select(t => new TitheListResponseDto(
                t.Id,
                t.Member.Name,
                t.Amount,
                t.ForYear,
                t.ForMonth,
                t.PaymentMethod,
                t.CollectionDate
            ))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<TitheListResponseDto>(records, paginationParameters, totalRecords);
    }

    public async Task<TitheResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(t => t.Id == id)
            .Select(t => new TitheResponseDto(
                t.Id,
                t.MemberId,
                t.Member.Name,
                t.Amount,
                t.ForYear,
                t.ForMonth,
                t.PaymentMethod,
                t.CollectionDate,
                t.Description,
                t.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
