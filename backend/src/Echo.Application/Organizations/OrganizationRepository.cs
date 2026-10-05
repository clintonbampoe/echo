using Echo.Data;
using Echo.Domain.Organizations;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Organizations;

public class OrganizationRepository(AppDbContext context, TimeProvider timeProvider)
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
            .Filter(filters, timeProvider)
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

    public Task<int> Count(
        Guid congregationId,
        OrganizationFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<int> CountTotalMembers(
        Guid congregationId,
        OrganizationFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .SelectMany(o => _memberDbSet.FilterDeleted().Where(m => m.OrganizationId == o.Id))
            .CountAsync(ct);

    public Task<double> AverageMembersPerOrganization(
        Guid congregationId,
        OrganizationFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .Select(o => _memberDbSet.FilterDeleted().Count(m => m.OrganizationId == o.Id))
            .AverageAsync(count => (double?)count, ct)
            .ContinueWith(t => t.Result ?? 0d, ct);

    public Task<string?> LargestOrganization(
        Guid congregationId,
        OrganizationFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .Select(o => new
            {
                o.Name,
                MemberCount = _memberDbSet.FilterDeleted().Count(m => m.OrganizationId == o.Id),
            })
            .OrderByDescending(o => o.MemberCount)
            .Select(o => o.Name)
            .FirstOrDefaultAsync(ct);

    public void Create(Organization entity) => _dbSet.Add(entity);

    public void SoftDelete(Organization entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class OrganizationQueryExtensions
{
    internal static IQueryable<Organization> Filter(
        this IQueryable<Organization> query,
        OrganizationFilters filters,
        TimeProvider timeProvider
    )
    {
        if (filters.Name is not null)
            query = query.Where(o => EF.Functions.ILike(o.Name, $"%{filters.Name}%"));

        var from = filters.From ?? DateUtils.GetFirstDayOfWeek(timeProvider);
        var to = filters.To ?? DateUtils.GetDateToday(timeProvider);

        query = query.Where(o =>
            DateOnly.FromDateTime(o.CreatedAt) >= from && DateOnly.FromDateTime(o.CreatedAt) <= to
        );

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
