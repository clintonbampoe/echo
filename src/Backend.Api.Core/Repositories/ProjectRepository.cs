using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
            .Where(p => p.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .ApplyPagination(paginationParameters)
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
