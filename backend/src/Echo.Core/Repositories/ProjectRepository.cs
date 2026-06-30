using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
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
        var query = _dbSet
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
                CategoryName =  p.Category.Name,
                ManagerName =   p.Manager.Name,
                Name = p.Name,
                TargetAmount = p.TargetAmount,
                Status =  p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            })
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
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                CategoryId = p.Category.Id,
                CategoryName = p.Category.Name,
                ManagerId = p.Manager.Id,
                ManagerName = p.Manager.Name,
                Name = p.Name,
                TargetAmount = p.TargetAmount,
                Status =  p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Description =  p.Description,
                CreatedAt =  p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
