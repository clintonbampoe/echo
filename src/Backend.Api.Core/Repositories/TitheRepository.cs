using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class TitheRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<Tithe>(context: context, mapper)
{
    public async Task<List<TitheMonthlyAggregateDto>> GetAggregatedSummaryByMonth(
        Guid congregationId,
        int year,
        CancellationToken ct
    )
    {
        var summary = await _context
            .Tithes.Where(t =>
                t.CongregationId == congregationId && t.ForYear == year && t.DeletedAt == null
            )
            .GroupBy(t => t.ForMonth)
            .Select(g => new TitheMonthlyAggregateDto
            {
                Month = g.Key,
                Total = g.Sum(t => t.Amount),
            })
            .ToListAsync(ct);

        var sorted = summary
            .OrderBy(s => s.Month)
            .Select(s => new TitheMonthlyAggregateDto { Month = s.Month, Total = s.Total })
            .ToList();

        return sorted;
    }

    public override async Task<PagedResponse<Tithe>> GetPageAsync(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var totalRecords = await _dbSet.AsNoTracking().CountAsync(ct);

        var records = await _dbSet
            .AsNoTracking()
            .Include(a => a.Member)
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<Tithe>(records, paginationParameters, totalRecords);
    }

    public override async Task<Tithe?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Include(a => a.Member)
            .FirstOrDefaultAsync(ct);
    }
}
