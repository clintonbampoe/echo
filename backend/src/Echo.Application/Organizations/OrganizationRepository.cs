using Echo.Data;
using Echo.Domain.Organizations;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Shared.Utilities;

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

    // The organization ids come from a query already limited to the congregation, so the members
    // are not limited again: a congregation filter would send the database to the table for every
    // row instead of counting from the (organization, role) index.
    public async Task<Dictionary<Guid, int>> GetMemberCounts(
        IReadOnlyList<Guid> organizationIds,
        CancellationToken ct = default
    )
    {
        if (organizationIds.Count == 0)
            return [];

        return await _memberDbSet
            .AsNoTracking()
            .CountPerOrganization(organizationIds)
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
    }

    public async Task<Dictionary<Guid, string>> GetLeaders(
        IReadOnlyList<Guid> organizationIds,
        CancellationToken ct = default
    )
    {
        if (organizationIds.Count == 0)
            return [];

        var leaders = await _memberDbSet
            .AsNoTracking()
            .Leaders(organizationIds)
            .ToListAsync(ct);
        return OrganizationService.FirstLeaders(leaders);
    }

    public async Task<Dictionary<Guid, double>> GetAverageAges(
        IReadOnlyList<Guid> organizationIds,
        CancellationToken ct = default
    )
    {
        if (organizationIds.Count == 0)
            return [];

        var today = DateUtils.GetDateToday(timeProvider);
        return await _memberDbSet
            .AsNoTracking()
            .AverageAges(organizationIds, today)
            .ToDictionaryAsync(x => x.Id, x => x.Years, ct);
    }

    public async Task<OrganizationStats> GetSummaryStats(
        Guid congregationId,
        OrganizationFilters filters,
        CancellationToken ct = default
    )
    {
        var sizes = await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(o => o.CongregationId == congregationId)
            .Filter(filters)
            .Sizes(_memberDbSet.FilterDeleted())
            .ToListAsync(ct);
        return OrganizationStats.From(sizes);
    }

    public void Create(Organization entity) => _dbSet.Add(entity);

    public void SoftDelete(Organization entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record OrganizationStats(
    int Count,
    int TotalMembers,
    double AverageMembers,
    string? Largest,
    int LargestMembers
)
{
    // The figures are worked out from each organization's size, so one scan answers all of them.
    // A tie for the largest goes to the name that sorts first, so the answer is stable.
    public static OrganizationStats From(IReadOnlyList<OrganizationSize> sizes)
    {
        var total = sizes.Sum(s => s.Members);
        var largest = sizes
            .OrderByDescending(s => s.Members)
            .ThenBy(s => s.Name)
            .FirstOrDefault();

        return new OrganizationStats(
            sizes.Count,
            total,
            sizes.Count == 0 ? 0d : (double)total / sizes.Count,
            largest?.Name,
            largest?.Members ?? 0
        );
    }
}

public record OrganizationSize(string Name, int Members);

public record LeaderRow(Guid OrganizationId, string Name, DateOnly JoinedAt);

public record OrganizationAge(Guid Id, double Years);

internal static class OrganizationQueryExtensions
{
    internal static IQueryable<ParentCount> CountPerOrganization(
        this IQueryable<OrganizationMember> query,
        IReadOnlyList<Guid> organizationIds
    ) =>
        query
            .FilterDeleted()
            .Where(m => organizationIds.Contains(m.OrganizationId))
            .GroupBy(m => m.OrganizationId)
            .Select(g => new ParentCount(g.Key, g.Count()));

    internal static IQueryable<OrganizationSize> Sizes(
        this IQueryable<Organization> organizations,
        IQueryable<OrganizationMember> members
    ) =>
        organizations.Select(o => new OrganizationSize(
            o.Name,
            members.Count(m => m.OrganizationId == o.Id)
        ));

    internal static IQueryable<LeaderRow> Leaders(
        this IQueryable<OrganizationMember> members,
        IReadOnlyList<Guid> organizationIds
    ) =>
        members
            .FilterDeleted()
            .Where(m => organizationIds.Contains(m.OrganizationId) && m.Role == MemberRole.Leader)
            .Select(m => new LeaderRow(m.OrganizationId, m.Member.Person.Name, m.JoinedAt));

    // Whole years on the day, by the same rule as the member summary: a birthday not yet reached
    // this year takes a year off.
    internal static IQueryable<OrganizationAge> AverageAges(
        this IQueryable<OrganizationMember> members,
        IReadOnlyList<Guid> organizationIds,
        DateOnly today
    )
    {
        var (year, month, day) = (today.Year, today.Month, today.Day);

        return members
            .FilterDeleted()
            .Where(m => organizationIds.Contains(m.OrganizationId))
            .GroupBy(m => m.OrganizationId)
            .Select(g => new OrganizationAge(
                g.Key,
                g.Average(m =>
                    (double)(
                        year
                        - m.Member.DateOfBirth.Year
                        - (
                            m.Member.DateOfBirth.Month > month
                            || (m.Member.DateOfBirth.Month == month && m.Member.DateOfBirth.Day > day)
                                ? 1
                                : 0
                        )
                    )
                )
            ));
    }

    internal static IQueryable<Organization> Filter(
        this IQueryable<Organization> query,
        OrganizationFilters filters
    )
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.Name.Trim())}%";
            query = query.Where(o => EF.Functions.ILike(o.Name, pattern));
        }

        if (filters.Kind is not null)
            query = query.Where(o => o.Kind == filters.Kind);

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
        // The leading >= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every earlier name.
        if (cursor is not null)
            query = query.Where(o =>
                string.Compare(o.Name, cursor.Name) >= 0
                && (string.Compare(o.Name, cursor.Name) > 0 || o.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
