using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
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
        var query = DbSet
            .AsNoTracking()
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
                JoinedAt = o.JoinedAt,
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
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .Where(o => o.Id == id && o.CongregationId == congregationId)
            .Select(o => new OrganizationMemberResponseDto
            {
                Id = o.Id,
                MemberId = o.MemberId,
                MemberName = o.Member.Name,
                OrganizationId = o.OrganizationId,
                OrganizationName = o.Organization.Name,
                Role = o.Role,
                JoinedAt = o.JoinedAt,
                CreatedAt = o.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResponse<OrganizationMemberListResponseDto>> GetByMemberId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .Where(o => o.MemberId == memberId);

        var totalCount = await query.CountAsync(ct);

        var records = await query
            .OrderBy(o => o.Id)
            .Select(o => new OrganizationMemberListResponseDto
            {
                Id = o.Id,
                MemberName = o.Member.Name,
                OrganizationName = o.Organization.Name,
                Role = o.Role,
                JoinedAt = o.JoinedAt,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<OrganizationMemberListResponseDto>(
            records,
            paginationParameters,
            totalCount
        );
    }

    public async Task<PagedResponse<OrganizationMemberListResponseDto>> GetByOrganizationId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid organizationId,
        CancellationToken ct
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .Where(o => o.OrganizationId == organizationId);

        var totalCount = await query.CountAsync(ct);

        var records = await query
            .OrderBy(o => o.Id)
            .Select(o => new OrganizationMemberListResponseDto
            {
                Id = o.Id,
                MemberName = o.Member.Name,
                OrganizationName = o.Organization.Name,
                Role = o.Role,
                JoinedAt = o.JoinedAt,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<OrganizationMemberListResponseDto>(
            records,
            paginationParameters,
            totalCount
        );
    }
}
