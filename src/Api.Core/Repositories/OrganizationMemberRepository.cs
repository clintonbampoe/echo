using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class OrganizationMemberRepository(AppDbContext context)
    : PrimaryRepositoryBase<OrganizationMember>(context)
{
    public async Task<PagedResponse<OrganizationMemberListResponseDto>> GetPageAsync(
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
            .Where(o => o.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(o => o.Id)
            .Select(o => new OrganizationMemberListResponseDto(
                o.Id,
                o.Member.Name,
                o.Organization.Name,
                o.Role,
                o.JoinedAt
            ))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<OrganizationMemberListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<OrganizationMemberResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(o => o.Id == id)
            .Select(o => new OrganizationMemberResponseDto(
                o.Id,
                o.MemberId,
                o.Member.Name,
                o.OrganizationId,
                o.Organization.Name,
                o.Role,
                o.JoinedAt,
                o.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
