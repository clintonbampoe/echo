using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class OrganizationMemberRepository(AppDbContext context)
{
    private readonly DbSet<OrganizationMember> _dbSet = context.Set<OrganizationMember>();

    public async Task<List<OrganizationMember>> GetPage(
        Guid congregationId,
        OrganizationMemberFilters filters,
        OrganizationMemberCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Include(o => o.Member)
            .Include(o => o.Organization)
            .Filter(filters)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
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

    public async Task<List<OrganizationMember>> ListByMemberId(
        Guid congregationId,
        Guid memberId,
        OrganizationMemberFilters filters,
        OrganizationMemberCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Where(o => o.MemberId == memberId)
            .Include(o => o.Member)
            .Include(o => o.Organization)
            .Filter(filters)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<OrganizationMember>> ListByOrganizationId(
        Guid congregationId,
        Guid organizationId,
        OrganizationMemberFilters filters,
        OrganizationMemberCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Where(o => o.OrganizationId == organizationId)
            .Include(o => o.Member)
            .Include(o => o.Organization)
            .Filter(filters)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }
}

internal static class OrganizationMemberQueryExtensions
{
    internal static IQueryable<OrganizationMember> Filter(
        this IQueryable<OrganizationMember> query,
        OrganizationMemberFilters filters
    )
    {
        if (filters.Role is not null)
            query = query.Where(o => o.Role == filters.Role);

        return query;
    }

    internal static IQueryable<OrganizationMember> Paginate(
        this IQueryable<OrganizationMember> query,
        OrganizationMemberCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(e =>
                e.CreatedAt < cursor.CreatedAt
                || (e.CreatedAt == cursor.CreatedAt && e.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
