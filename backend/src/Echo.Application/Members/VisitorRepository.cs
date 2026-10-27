using Echo.Application.Attendances;
using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Domain.Members;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;
using Echo.Domain.Congregations;

namespace Echo.Application.Members;

public class VisitorRepository(AppDbContext context)
{
    private readonly DbSet<Visitor> _dbSet = context.Set<Visitor>();

    public async Task<List<Visitor>> List(
        Guid congregationId,
        VisitorFilters filters,
        VisitorCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(v => v.CongregationId == congregationId)
            // A visitor's person is always in the same congregation. Saying so lets the database
            // read the people in name order from the (congregation, name) index.
            .Where(v => v.Person.CongregationId == congregationId)
            .Include(v => v.Person)
            .Include(v => v.InvitedBy)
                .ThenInclude(m => m.Person)
            .Filter(filters)
            .OrderBy(v => v.Person.Name)
            .ThenBy(v => v.PersonId)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Visitor?> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(v => v.PersonId == id && v.CongregationId == congregationId)
            .Include(v => v.Person)
            .Include(v => v.InvitedBy)
                .ThenInclude(m => m.Person)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Visitor>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(v => v.CongregationId == congregationId)
            .Include(v => v.Person)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, (int Count, DateOnly LastVisited)>> GetVisitStats(
        Guid congregationId,
        IReadOnlyList<Guid> personIds,
        CancellationToken ct = default
    )
    {
        if (personIds.Count == 0)
            return [];

        return await context
            .Set<Attendance>()
            .AsNoTracking()
            .Where(a => a.CongregationId == congregationId)
            .VisitStatsFor(personIds)
            .ToDictionaryAsync(x => x.PersonId, x => (x.Count, x.LastDate), ct);
    }

    public async Task<VisitorStats> GetSummaryStats(
        Guid congregationId,
        VisitorFilters filters,
        CancellationToken ct = default
    )
    {
        var everyone = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(v => v.CongregationId == congregationId);
        var named = everyone.WithName(filters.Name);
        var attendance = context
            .Set<Attendance>()
            .Live()
            .Where(a => a.CongregationId == congregationId);

        var stats = await context
            .OneRow(congregationId)
            .SummarizeVisitors(
                everyone.Filter(filters),
                named.CreatedBetween(filters.From, filters.To),
                named.ConvertedBetween(filters.From, filters.To),
                attendance
            )
            .FirstOrDefaultAsync(ct);
        return stats ?? new VisitorStats(0, 0, 0, 0, 0, 0, 0);
    }

    public void Create(Visitor entity) => _dbSet.Add(entity);

    public void SoftDelete(Visitor entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record VisitorStats(
    int Total,
    int New,
    int Converted,
    int Recurring,
    int Invited,
    int Male,
    int Female
);

internal readonly record struct VisitStats(Guid PersonId, int Count, DateOnly LastDate);

internal static class VisitorQueryExtensions
{
    /// <summary>Visits are attendance records made while the person was a visitor.</summary>
    internal static IQueryable<VisitStats> VisitStatsFor(
        this IQueryable<Attendance> query,
        IReadOnlyList<Guid> personIds
    ) =>
        query
            .Live()
            .Where(a => personIds.Contains(a.PersonId) && a.PersonKindAtTime == PersonKind.Visitor)
            .GroupBy(a => a.PersonId)
            .Select(g => new VisitStats(g.Key, g.Count(), g.Max(a => a.SessionDate)));

    // visitors is the visitor list for the filters; created and converted are everyone created or
    // converted in the window, whatever their current status.
    internal static IQueryable<VisitorStats> SummarizeVisitors(
        this IQueryable<Congregation> oneRow,
        IQueryable<Visitor> visitors,
        IQueryable<Visitor> created,
        IQueryable<Visitor> converted,
        IQueryable<Attendance> attendance
    ) =>
        oneRow.Select(_ => new VisitorStats(
            visitors.Count(),
            created.Count(),
            converted.Count(),
            // Came more than once: has a visit as a visitor that was not their first.
            visitors.Count(v =>
                attendance.Any(a =>
                    a.PersonId == v.PersonId
                    && a.PersonKindAtTime == PersonKind.Visitor
                    && !a.IsFirstVisit
                )
            ),
            visitors.Count(v => v.Source == VisitorSource.Invited),
            visitors.Count(v => v.Gender == Gender.Male),
            visitors.Count(v => v.Gender == Gender.Female)
        ));

    internal static IQueryable<Visitor> Filter(
        this IQueryable<Visitor> query,
        VisitorFilters filters
    )
    {
        query = query.WithName(filters.Name);

        if (filters.Gender is not null)
            query = query.Where(v => v.Gender == filters.Gender);

        if (filters.Source is not null)
            query = query.Where(v => v.Source == filters.Source);

        if (filters.InvitedByMemberId is not null)
            query = query.Where(v => v.InvitedByMemberId == filters.InvitedByMemberId);

        // Default is "current visitors only" (ConvertedAt IS NULL), windowed by when they were created.
        // Pass Converted = true for the historical view: converted visitors, windowed by when they
        // were converted. Converted = false is the same as the default.
        return filters.Converted == true
            ? query.ConvertedBetween(filters.From, filters.To)
            : query.Where(v => v.ConvertedAt == null).CreatedBetween(filters.From, filters.To);
    }

    internal static IQueryable<Visitor> WithName(this IQueryable<Visitor> query, string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? query
            : query.Where(v =>
                EF.Functions.ILike(
                    v.Person.Name,
                    $"%{Echo.Shared.Query.Extensions.EscapeLike(name.Trim())}%"
                )
            );

    internal static IQueryable<Visitor> CreatedBetween(
        this IQueryable<Visitor> query,
        DateOnly? from,
        DateOnly? to
    )
    {
        if (from is not null)
        {
            var start = StartOf(from.Value);
            query = query.Where(v => v.CreatedAt >= start);
        }

        if (to is not null)
        {
            var end = EndOf(to.Value);
            query = query.Where(v => v.CreatedAt < end);
        }

        return query;
    }

    internal static IQueryable<Visitor> ConvertedBetween(
        this IQueryable<Visitor> query,
        DateOnly? from,
        DateOnly? to
    )
    {
        query = query.Where(v => v.ConvertedAt != null);

        if (from is not null)
        {
            var start = StartOf(from.Value);
            query = query.Where(v => v.ConvertedAt >= start);
        }

        if (to is not null)
        {
            var end = EndOf(to.Value);
            query = query.Where(v => v.ConvertedAt < end);
        }

        return query;
    }

    // A day as a range of instants, so the database can seek the timestamp index instead of casting
    // every row to a date.
    private static DateTime StartOf(DateOnly day) => day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    private static DateTime EndOf(DateOnly day) =>
        day == DateOnly.MaxValue ? DateTime.MaxValue : StartOf(day.AddDays(1));

    internal static IQueryable<Visitor> Paginate(
        this IQueryable<Visitor> query,
        VisitorCursor? cursor,
        int pageSize
    )
    {
        // The leading >= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every earlier name.
        if (cursor is not null)
            query = query.Where(v =>
                string.Compare(v.Person.Name, cursor.Name) >= 0
                && (
                    string.Compare(v.Person.Name, cursor.Name) > 0
                    || v.PersonId > cursor.Id
                )
            );

        return query.Take(pageSize);
    }

    internal static IQueryable<Visitor> SearchName(this IQueryable<Visitor> query, string name)
    {
        var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(name.Trim())}%";
        return query
            .Where(v => EF.Functions.ILike(v.Person.Name, pattern))
            .OrderByDescending(v => EF.Functions.TrigramsSimilarity(v.Person.Name, name))
            .Take(5);
    }
}
