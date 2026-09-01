using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

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
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .ApplySearchFilter(queryParameters)
            .Where(o => o.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(o => o.Id)
            .Select(o => new OrganizationListResponseDto
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
            })
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
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .Where(o => o.Id == id && o.CongregationId == congregationId)
            .Select(o => new OrganizationResponseDto
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<OrganizationSummaryDto> GetSummaryAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var currentMonthStart = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc
        );

        var organizations = DbSet
            .Where(o => o.CongregationId == congregationId);

        var totalOrganizations = await organizations.CountAsync(ct);
        var newOrganizationsThisMonth = await organizations.CountAsync(
            o => o.CreatedAt >= currentMonthStart,
            ct
        );

        var totalOrganizationMembers = await Context
            .Set<OrganizationMember>()
            .Where(m => m.CongregationId == congregationId)
            .CountAsync(ct);

        return new OrganizationSummaryDto
        {
            TotalOrganizations = totalOrganizations,
            TotalOrganizationMembers = totalOrganizationMembers,
            NewOrganizationsThisMonth = newOrganizationsThisMonth,
            AverageMembersPerOrganization =
                totalOrganizations == 0
                    ? 0
                    : Math.Round((decimal)totalOrganizationMembers / totalOrganizations, 1),
        };
    }
}
