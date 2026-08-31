using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class ProjectContributionRepository(AppDbContext context)
    : PrimaryRepositoryBase<ProjectContribution>(context)
{
    public async Task<PagedResponse<ProjectContributionListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .Where(p => p.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(p => p.Id)
            .Select(p => new ProjectContributionListResponseDto
            {
                Id = p.Id,
                ProjectName = p.Project.Name,
                Amount = p.Amount,
                DateContributed = p.DateContributed,
                PaymentMethod = p.PaymentMethod,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<ProjectContributionListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<ProjectContributionResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .Select(p => new ProjectContributionResponseDto
            {
                Id = p.Id,
                ProjectId = p.Project.Id,
                ProjectName = p.Project.Name,
                Amount = p.Amount,
                DateContributed = p.DateContributed,
                PaymentMethod = p.PaymentMethod,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ProjectContributionSummaryDto?> GetSummaryAsync(
        Guid congregationId,
        Guid projectId,
        CancellationToken ct = default
    )
    {
        var project = await Context
            .Set<Project>()
            .Where(p => p.CongregationId == congregationId && p.Id == projectId)
            .Select(p => new { p.TargetAmount })
            .FirstOrDefaultAsync(ct);

        if (project is null)
            return null;

        var stats = await DbSet
            .Where(c => c.CongregationId == congregationId && c.ProjectId == projectId)
            .GroupBy(c => 1)
            .Select(g => new
            {
                TotalRaised = g.Sum(c => c.Amount),
                Contributors = g.Count(),
                MostRecent = g.Max(c => c.DateContributed),
            })
            .FirstOrDefaultAsync(ct);

        return new ProjectContributionSummaryDto
        {
            TotalRaised = stats?.TotalRaised ?? 0,
            TargetGoal = project.TargetAmount,
            Contributors = stats?.Contributors ?? 0,
            MostRecentEntryDate = stats?.MostRecent,
        };
    }
}
