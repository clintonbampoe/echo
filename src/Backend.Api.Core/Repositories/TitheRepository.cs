using AutoMapper;
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
                Total = g.Sum(t => t.Decimal),
            })
            .ToListAsync(ct);

        return summary;
    }
}
