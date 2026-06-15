using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
            .Where(p => p.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .ApplyPagination(paginationParameters)
            .Select(p => new ProjectContributionListResponseDto(
                p.Id,
                p.Project.Name,
                p.Amount,
                p.DateContributed,
                p.PaymentMethod
            ))
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
            .Select(p => new ProjectContributionResponseDto(
                p.Id,
                p.ProjectId,
                p.Project.Name,
                p.Amount,
                p.DateContributed,
                p.PaymentMethod,
                p.Description,
                p.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
