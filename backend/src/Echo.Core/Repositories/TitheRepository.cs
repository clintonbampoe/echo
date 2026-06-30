using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

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
            .Select(t => new TitheListResponseDto
            {
                Id = t.Id,
                MemberName =  t.Member.Name,
                Amount = t.Amount,
                ForYear = t.ForYear,
                ForMonth = t.ForMonth,
                PaymentMethod = t.PaymentMethod,
                CollectionDate = t.CollectionDate
            })
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
            .Select(t => new TitheResponseDto
            {
                Id = t.Id,
                MemberId =  t.MemberId,
                MemberName = t.Member.Name,
                Amount = t.Amount,
                ForYear = t.ForYear,
                ForMonth = t.ForMonth,
                PaymentMethod = t.PaymentMethod,
                CollectionDate = t.CollectionDate,
                Description =   t.Description,
                CreatedAt =  t.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
