using Echo.Data;
using Echo.Domain.Organizations;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Domain.Congregations;

namespace Echo.Application.Organizations;

public class OrganizationRepository(AppDbContext context)
{
    private readonly DbSet<Organization> _dbSet = context.Set<Organization>();
    private readonly DbSet<OrganizationMember> _memberDbSet = context.Set<OrganizationMember>();

    public async Task<List<Organization>> List(
        Guid congregationId,
        OrganizationFilters filters,
        OrganizationCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Filter(filters)
            .OrderBy(o => o.Name)
            .ThenBy(o => o.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Organization?> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterDeleted()
            .Where(o => o.Id == id && o.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Organization>> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(o => o.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, int>> GetMemberCounts(
        Guid congregationId,
        IReadOnlyList<Guid> organizationIds,
        CancellationToken ct = default
    )
    {
        if (organizationIds.Count == 0)
            return [];

        return await context
            .Set<OrganizationMember>()
            .AsNoTracking()
            .Where(m => m.CongregationId == congregationId)
            .CountPerOrganization(organizationIds)
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
    }

    public async Task<OrganizationStats> GetSummaryStats(
        Guid congregationId,
        OrganizationFilters filters,
        CancellationToken ct = default
    )
    {
        var organizations = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Filter(filters);

        var stats = await context
            .OneRow(congregationId)
            .SummarizeOrganizations(organizations, _memberDbSet.FilterDeleted())
            .FirstOrDefaultAsync(ct);
        return stats ?? new OrganizationStats(0, 0, 0d, null);
    }

    public void Create(Organization entity) => _dbSet.Add(entity);

    public void SoftDelete(Organization entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class OrganizationQueryExtensions
{
    internal static IQueryable<OrganizationStats> SummarizeOrganizations(
        this IQueryable<Congregation> oneRow,
        IQueryable<Organization> organizations,
        IQueryable<OrganizationMember> members
    ) =>
        oneRow.Select(_ => new OrganizationStats(
            organizations.Count(),
            organizations.SelectMany(o => members.Where(m => m.OrganizationId == o.Id)).Count(),
            organizations
                .Select(o => members.Count(m => m.OrganizationId == o.Id))
                .Average(count => (double?)count) ?? 0d,
            organizations
                .Select(o => new { o.Name, Count = members.Count(m => m.OrganizationId == o.Id) })
                .OrderByDescending(o => o.Count)
                .Select(o => o.Name)
                .FirstOrDefault()
        ));

    internal static IQueryable<Organization> Filter(
        this IQueryable<Organization> query,
        OrganizationFilters filters
    )
    {
        if (filters.Name is not null)
            query = query.Where(o => EF.Functions.ILike(o.Name, $"%{filters.Name}%"));

        if (filters.From is not null)
            query = query.Where(o => DateOnly.FromDateTime(o.CreatedAt) >= filters.From);

        if (filters.To is not null)
            query = query.Where(o => DateOnly.FromDateTime(o.CreatedAt) <= filters.To);

        return query;
    }

    internal static IQueryable<Organization> Paginate(
        this IQueryable<Organization> query,
        OrganizationCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(o =>
                string.Compare(o.Name, cursor.Name) > 0
                || (o.Name == cursor.Name && o.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
