using Echo.Application.Extensions.QueryExtensions;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class OrganizationMemberRepository(AppDbContext context)
{
    private readonly DbSet<OrganizationMember> _dbSet = context.Set<OrganizationMember>();

    public async Task<List<OrganizationMember>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(o => o.CongregationId == congregationId);

        var res = await query
            .OrderBy(o => o.Id)
            .Include(o => o.Member)
            .Include(o => o.Organization)
            .ToListAsync(ct);

        return res;
    }

    public async Task<OrganizationMember?> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(o => o.Id == id && o.CongregationId == congregationId)
            .Include(o => o.Member)
            .Include(o => o.Organization)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(OrganizationMember entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(OrganizationMember entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public async Task<List<OrganizationMember>> GetByMemberId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(o => o.MemberId == memberId);

        var res = await query
            .OrderBy(o => o.Organization.Name)
            .Include(o => o.Member)
            .Include(o => o.Organization)
            .ToListAsync(ct);

        return res;
    }

    public async Task<List<OrganizationMember>> GetByOrganizationId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid organizationId,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(o => o.OrganizationId == organizationId);

        var res = await query
            .OrderBy(o => o.Organization.Name)
            .Include(o => o.Member)
            .Include(o => o.Organization)
            .ToListAsync(ct);

        return res;
    }
}
