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

public class ProjectRepository(AppDbContext context) : PrimaryRepositoryBase<Project>(context)
{
    public async Task<PagedResponse<ProjectListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(p => p.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(p => p.Id)
            .Select(p => new ProjectListResponseDto
            {
                Id = p.Id,
                CategoryName = p.Category.Name,
                ManagerName = p.Manager.Name,
                Name = p.Name,
                TargetAmount = p.TargetAmount,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<ProjectListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<ProjectResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                CategoryId = p.Category.Id,
                CategoryName = p.Category.Name,
                ManagerId = p.Manager.Id,
                ManagerName = p.Manager.Name,
                Name = p.Name,
                TargetAmount = p.TargetAmount,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ProjectSummaryDto> GetSummaryAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var now = DateTime.UtcNow;
        var currentQuarterStart = new DateOnly(now.Year, (now.Month - 1) / 3 * 3 + 1, 1);
        var currentQuarterEnd = currentQuarterStart.AddMonths(3);

        // GroupBy(1) forces every matching row into one bucket, letting us compute
        // the sum + both counts below in a single SQL query instead of 3 separate round trips.
        var projectStats = await DbSet
            .ApplySoftDeleteFilter()
            .Where(p => p.CongregationId == congregationId)
            .GroupBy(p => 1)
            .Select(g => new
            {
                TotalExpected = g.Sum(p => p.TargetAmount),
                ActiveProjects = g.Count(p =>
                    p.Status == ProjectStatus.OnTrack || p.Status == ProjectStatus.AtRisk
                ),
                CompletedThisQuarter = g.Count(p =>
                    p.Status == ProjectStatus.Complete
                    && p.EndDate != null
                    && p.EndDate >= currentQuarterStart
                    && p.EndDate < currentQuarterEnd
                ),
            })
            .FirstOrDefaultAsync(ct);

        var totalRaised = await Context
            .Set<ProjectContribution>()
            .ApplySoftDeleteFilter()
            .Where(c => c.CongregationId == congregationId)
            .SumAsync(c => c.Amount, ct);

        return new ProjectSummaryDto
        {
            ActiveProjects = projectStats?.ActiveProjects ?? 0,
            TotalRaised = totalRaised,
            TotalExpected = projectStats?.TotalExpected ?? 0,
            CompletedThisQuarter = projectStats?.CompletedThisQuarter ?? 0,
        };
    }
}
