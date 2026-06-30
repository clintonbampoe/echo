using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

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
            .Select(o => new OrganizationMemberListResponseDto
            {
                Id = o.Id,
                MemberName = o.Member.Name,
                OrganizationName = o.Organization.Name,
                Role = o.Role,
                JoinedAt = o.JoinedAt
            })
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
            .Select(o => new OrganizationMemberResponseDto
            {
                Id = o.Id,
                MemberId = o.MemberId,
                MemberName =  o.Member.Name,
                OrganizationId = o.OrganizationId,
                OrganizationName = o.Organization.Name,
                Role = o.Role,
                JoinedAt = o.JoinedAt,
                CreatedAt = o.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
