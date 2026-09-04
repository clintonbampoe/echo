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

public class TitheRepository(AppDbContext context) : PrimaryRepositoryBase<Tithe>(context)
{
    public async Task<PagedResponse<TitheListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = DbSet
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
                MemberName = t.Member.Name,
                Amount = t.Amount,
                ForYear = t.ForYear,
                ForMonth = t.ForMonth,
                PaymentMethod = t.PaymentMethod,
                CollectionDate = t.CollectionDate,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<TitheListResponseDto>(records, paginationParameters, totalRecords);
    }

    public async Task<TitheResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Select(t => new TitheResponseDto
            {
                Id = t.Id,
                MemberId = t.MemberId,
                MemberName = t.Member.Name,
                Amount = t.Amount,
                ForYear = t.ForYear,
                ForMonth = t.ForMonth,
                PaymentMethod = t.PaymentMethod,
                CollectionDate = t.CollectionDate,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<TitheMonthlyTotalDto>> GetMonthlySummaryAsync(
        Guid congregationId,
        int year,
        CancellationToken ct = default
    )
    {
        var byMonth = await DbSet
            .ApplySoftDeleteFilter()
            .Where(t => t.CongregationId == congregationId && t.ForYear == year)
            .GroupBy(t => t.ForMonth)
            .Select(g => new { Month = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(t => t.Month, t => t.Total, ct);

        return Enum.GetValues<MonthOfYear>()
            .Distinct()
            .Select(m => new TitheMonthlyTotalDto
            {
                Month = m,
                Total = byMonth.GetValueOrDefault(m, 0m),
            })
            .ToList();
    }
}
