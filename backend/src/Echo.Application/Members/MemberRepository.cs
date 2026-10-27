using Echo.Data;
using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Members;

public class MemberRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<Member> _dbSet = context.Set<Member>();
    private readonly DbSet<OrganizationMember> _organizationMemberDbSet =
        context.Set<OrganizationMember>();

    public async Task<List<Member>> List(
        Guid congregationId,
        MemberFilters filters,
        MemberCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await Scope(congregationId, filters)
            // A member's person is always in the same congregation. Saying so lets the database
            // read the people in name order from the (congregation, name) index.
            .Where(m => m.Person.CongregationId == congregationId)
            .Include(m => m.Person)
            .OrderBy(m => m.Person.Name)
            .ThenBy(m => m.PersonId)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Member?> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(m => m.PersonId == id && m.CongregationId == congregationId)
            .Include(m => m.Person)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Member>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(m => m.CongregationId == congregationId)
            .Include(m => m.Person)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public async Task<MemberStats> GetSummaryStats(
        Guid congregationId,
        MemberFilters filters,
        CancellationToken ct = default
    )
    {
        var today = DateUtils.GetDateToday(timeProvider);

        // One query, one pass: every count below is a FILTER aggregate over the same scan.
        var stats = await Scope(congregationId, filters)
            .SummarizeMembers(today)
            .FirstOrDefaultAsync(ct);

        return stats ?? new MemberStats(0, 0, 0, 0, 0d, 0, 0, 0, 0, 0, 0);
    }

    // How many members joined in the window, in the window before it, and this calendar year. The
    // filters' dates are left out: they say which members to list, not which joins to count.
    public async Task<JoinedCounts> GetJoinedCounts(
        Guid congregationId,
        MemberFilters filters,
        DateWindow window,
        CancellationToken ct = default
    )
    {
        var today = DateUtils.GetDateToday(timeProvider);
        var counts = await Scope(congregationId, filters with { From = null, To = null })
            .JoinedCounts(window, window.Previous(), new DateOnly(today.Year, 1, 1), today)
            .FirstOrDefaultAsync(ct);

        return counts ?? new JoinedCounts(0, 0, 0);
    }

    private IQueryable<Member> Scope(Guid congregationId, MemberFilters filters)
    {
        var members = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(m => m.CongregationId == congregationId)
            .Filter(filters);

        if (filters.OrganizationId is { } organizationId)
            members = members.Where(m =>
                _organizationMemberDbSet.Any(om =>
                    om.MemberId == m.PersonId
                    && om.OrganizationId == organizationId
                    && om.DeletedAt == null
                )
            );

        return members;
    }

    public async Task<List<MemberMonthRow>> GetMonthlyTotals(
        Guid congregationId,
        MemberMonthlyFilters filters,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var members = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(m => m.CongregationId == congregationId)
            .Where(m => m.JoinedDate >= from && m.JoinedDate <= to);

        if (filters.Status is not null)
            members = members.Where(m => m.Status == filters.Status);

        if (filters.Gender is not null)
            members = members.Where(m => m.Gender == filters.Gender);

        return await members.JoinedByMonth().ToListAsync(ct);
    }

    public void Create(Member entity) => _dbSet.Add(entity);

    public void SoftDelete(Member entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record MemberMonthRow(int Year, int Month, int Count);

public record JoinedCounts(int InWindow, int InPreviousWindow, int ThisYear);

public record MemberStats(
    int Total,
    int Active,
    int Male,
    int Female,
    double AverageAge,
    int Under18,
    int Age18To29,
    int Age30To44,
    int Age45To59,
    int Age60Plus,
    int Baptised
);

internal static class MemberQueryExtensions
{
    // Joins inside the window, inside the one before it, and since the start of the year (one scan).
    internal static IQueryable<JoinedCounts> JoinedCounts(
        this IQueryable<Member> members,
        DateWindow window,
        DateWindow previous,
        DateOnly yearStart,
        DateOnly today
    )
    {
        var earliest = previous.From < yearStart ? previous.From : yearStart;
        var latest = window.To > today ? window.To : today;

        return members
            .Where(m => m.JoinedDate >= earliest && m.JoinedDate <= latest)
            .GroupBy(_ => 1)
            .Select(g => new JoinedCounts(
                g.Count(m => m.JoinedDate >= window.From && m.JoinedDate <= window.To),
                g.Count(m => m.JoinedDate >= previous.From && m.JoinedDate <= previous.To),
                g.Count(m => m.JoinedDate >= yearStart && m.JoinedDate <= today)
            ));
    }

    // New members by the month they joined; members with no join date are not in any month.
    internal static IQueryable<MemberMonthRow> JoinedByMonth(this IQueryable<Member> members) =>
        members
            .Where(m => m.JoinedDate != null)
            .GroupBy(m => new { m.JoinedDate!.Value.Year, m.JoinedDate.Value.Month })
            .Select(g => new MemberMonthRow(g.Key.Year, g.Key.Month, g.Count()));

    // Reads only congregation_id, joined_date, status, gender and date_of_birth, all of which are in
    // the (congregation, joined_date) covering index on members, so the database can answer from
    // the index alone.
    internal static IQueryable<MemberStats> SummarizeMembers(
        this IQueryable<Member> query,
        DateOnly today
    )
    {
        var (year, month, day) = (today.Year, today.Month, today.Day);

        // Someone is at least N years old exactly when they were born on or before the day N years
        // ago, so each age group is a range of birth dates. These are worked out once here, not per
        // row, which keeps the scan cheap with millions of members.
        var born18 = today.AddYears(-18);
        var born30 = today.AddYears(-30);
        var born45 = today.AddYears(-45);
        var born60 = today.AddYears(-60);

        return query
            .GroupBy(m => m.CongregationId)
            .Select(group => new MemberStats(
                group.Count(),
                group.Count(m => m.Status == MemberStatus.Active),
                group.Count(m => m.Gender == Gender.Male),
                group.Count(m => m.Gender == Gender.Female),
                group.Average(m =>
                    (double?)(
                        year
                        - m.DateOfBirth.Year
                        - (
                            m.DateOfBirth.Month > month
                            || (m.DateOfBirth.Month == month && m.DateOfBirth.Day > day)
                                ? 1
                                : 0
                        )
                    )
                ) ?? 0d,
                group.Count(m => m.DateOfBirth > born18),
                group.Count(m => m.DateOfBirth <= born18 && m.DateOfBirth > born30),
                group.Count(m => m.DateOfBirth <= born30 && m.DateOfBirth > born45),
                group.Count(m => m.DateOfBirth <= born45 && m.DateOfBirth > born60),
                group.Count(m => m.DateOfBirth <= born60),
                group.Count(m => m.BaptisedDate != null)
            ));
    }

    internal static IQueryable<Member> Filter(this IQueryable<Member> query, MemberFilters filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.Name.Trim())}%";
            query = query.Where(m => EF.Functions.ILike(m.Person.Name, pattern));
        }

        if (filters.Status.HasValue)
            query = query.Where(m => m.Status == filters.Status.Value);

        if (filters.Gender.HasValue)
            query = query.Where(m => m.Gender == filters.Gender.Value);

        if (filters.Region.HasValue)
            query = query.Where(m => m.Region == filters.Region.Value);

        if (filters.MaritalStatus.HasValue)
            query = query.Where(m => m.MaritalStatus == filters.MaritalStatus.Value);

        // Members without a JoinedDate are kept in every window. This is one "undated, or inside the
        // window" test, rather than one test per bound, so the database sees a single range plus
        // the undated rows and can combine the two index lookups.
        if (filters.From is not null || filters.To is not null)
        {
            var from = filters.From ?? DateOnly.MinValue;
            var to = filters.To ?? DateOnly.MaxValue;
            query = query.Where(m =>
                m.JoinedDate == null || (m.JoinedDate >= from && m.JoinedDate <= to)
            );
        }

        return query;
    }

    internal static IQueryable<Member> Paginate(
        this IQueryable<Member> query,
        MemberCursor? cursor,
        int pageSize
    )
    {
        // The leading >= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every earlier name.
        if (cursor is not null)
            query = query.Where(m =>
                string.Compare(m.Person.Name, cursor.Name) >= 0
                && (
                    string.Compare(m.Person.Name, cursor.Name) > 0
                    || m.PersonId > cursor.Id
                )
            );

        return query.Take(pageSize);
    }

    internal static IQueryable<Member> SearchName(this IQueryable<Member> query, string name)
    {
        var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(name.Trim())}%";
        return query
            .Where(x => EF.Functions.ILike(x.Person.Name, pattern))
            .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.Person.Name, name))
            .Take(5);
    }
}
