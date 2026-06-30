using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
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
        var query = _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplyDateFilters(queryParameters)
            .Where(p => p.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(p => p.Id)
            .Select(p => new ProjectContributionListResponseDto
            {
                Id = p.Id,
                ProjectName =  p.Project.Name,
                Amount = p.Amount,
                DateContributed = p.DateContributed,
                PaymentMethod = p.PaymentMethod
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
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(p => p.Id == id)
            .Select(p => new ProjectContributionResponseDto
            {
                Id = p.Id,
                ProjectId =  p.Project.Id,
                ProjectName = p.Project.Name,
                Amount = p.Amount,
                DateContributed = p.DateContributed,
                PaymentMethod = p.PaymentMethod,
                Description = p.Description,
                CreatedAt =  p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
