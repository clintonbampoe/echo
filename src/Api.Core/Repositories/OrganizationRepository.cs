using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Api.Core.Dtos.Core;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class OrganizationRepository(AppDbContext context)
    : PrimaryRepositoryBase<Organization>(context)
{
    public async Task<PagedResponse<OrganizationListResponseDto>> GetPageAsync(
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
            .ApplySearchFilter(queryParameters)
            .Where(o => o.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(o => o.Id)
            .Select(o => new OrganizationListResponseDto(o.Id, o.Name, o.Description))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<OrganizationListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<OrganizationResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(o => o.Id == id)
            .Select(o => new OrganizationResponseDto(o.Id, o.Name, o.Description, o.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }
}
