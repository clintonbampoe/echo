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
            .Include(v => v.Person)
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
        var visitors = _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(v => v.CongregationId == congregationId)
            .Filter(filters);
        var attendance = context.Set<Attendance>().Live();

        var stats = await context
            .OneRow(congregationId)
            .SummarizeVisitors(visitors, attendance)
            .FirstOrDefaultAsync(ct);
        return stats ?? new VisitorStats(0, 0, 0);
    }

    public void Create(Visitor entity) => _dbSet.Add(entity);

    public void SoftDelete(Visitor entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class VisitorQueryExtensions
{
    internal static IQueryable<VisitorStats> SummarizeVisitors(
        this IQueryable<Congregation> oneRow,
        IQueryable<Visitor> visitors,
        IQueryable<Attendance> attendance
    ) =>
        oneRow.Select(_ => new VisitorStats(
            visitors.Count(),
            visitors.Count(v => v.ConvertedAt != null),
            visitors.Count(v => attendance.Count(a => a.PersonId == v.PersonId) > 1)
        ));

    internal static IQueryable<Visitor> Filter(
        this IQueryable<Visitor> query,
        VisitorFilters filters
    )
    {
        if (filters.Name is not null)
            query = query.Where(v => EF.Functions.ILike(v.Person.Name, $"%{filters.Name}%"));

        // Default is "current visitors only" (ConvertedAt IS NULL).
        // Pass Converted = true for the historical/converted view.
        // Converted = false is the same as the default — kept for explicit callers.
        query =
            filters.Converted == true
                ? query.Where(v => v.ConvertedAt != null)
                : query.Where(v => v.ConvertedAt == null);

        if (filters.Converted == true)
        {
            if (filters.From is not null)
                query = query.Where(v =>
                    DateOnly.FromDateTime(v.ConvertedAt!.Value) >= filters.From
                );

            if (filters.To is not null)
                query = query.Where(v =>
                    DateOnly.FromDateTime(v.ConvertedAt!.Value) <= filters.To
                );
        }
        else
        {
            if (filters.From is not null)
                query = query.Where(v => DateOnly.FromDateTime(v.CreatedAt) >= filters.From);

            if (filters.To is not null)
                query = query.Where(v => DateOnly.FromDateTime(v.CreatedAt) <= filters.To);
        }

        return query;
    }

    internal static IQueryable<Visitor> Paginate(
        this IQueryable<Visitor> query,
        VisitorCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(v =>
                string.Compare(v.Person.Name, cursor.Name) > 0
                || (v.Person.Name == cursor.Name && v.PersonId > cursor.Id)
            );

        return query.Take(pageSize);
    }

    internal static IQueryable<Visitor> SearchName(this IQueryable<Visitor> query, string name)
    {
        return query
            .Where(v => EF.Functions.ILike(v.Person.Name, $"%{name}%"))
            .OrderByDescending(v => EF.Functions.TrigramsSimilarity(v.Person.Name, name))
            .Take(5);
    }
}
