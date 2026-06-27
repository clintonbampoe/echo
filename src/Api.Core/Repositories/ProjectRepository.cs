using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class ProjectRepository(AppDbContext context) : PrimaryRepositoryBase<Project>(context)
{
    public async Task<PagedResponse<ProjectListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(p => p.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(p => p.Id)
            .Select(p => new ProjectListResponseDto(
                p.Id,
                p.Category.Name,
                p.Manager.Name,
                p.Name,
                p.TargetAmount,
                p.Status,
                p.StartDate,
                p.EndDate
            ))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<ProjectListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<ProjectResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(p => p.Id == id)
            .Select(p => new ProjectResponseDto(
                p.Id,
                p.CategoryId,
                p.Category.Name,
                p.ManagerId,
                p.Manager.Name,
                p.Name,
                p.TargetAmount,
                p.Status,
                p.StartDate,
                p.EndDate,
                p.Description,
                p.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
