using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Domain.Members;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Members;

public class VisitorRepository(AppDbContext context, TimeProvider timeProvider)
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
            .Include(v => v.ConvertedToMember)
                .ThenInclude(m => m!.Person)
            .Filter(filters, timeProvider)
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
            .Include(v => v.ConvertedToMember)
                .ThenInclude(m => m!.Person)
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

    public Task<int> Count(
        Guid congregationId,
        VisitorFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(v => v.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<int> CountNew(
        Guid congregationId,
        VisitorFilters filters,
        CancellationToken ct = default
    )
    {
        var from = filters.From ?? DateUtils.GetFirstDayOfWeek(timeProvider);
        var to = filters.To ?? DateUtils.GetDateToday(timeProvider);

        return _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(v =>
                v.CongregationId == congregationId
                && DateOnly.FromDateTime(v.CreatedAt) >= from
                && DateOnly.FromDateTime(v.CreatedAt) <= to
            )
            .Filter(filters, timeProvider)
            .CountAsync(ct);
    }

    public Task<int> CountRecurring(
        Guid congregationId,
        VisitorFilters filters,
        CancellationToken ct = default
    )
    {
        var attendanceDbSet = context.Set<Attendance>();
        return _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(v => v.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .Where(v => attendanceDbSet.FilterDeleted().Count(a => a.PersonId == v.PersonId) > 1)
            .CountAsync(ct);
    }

    public Task<int> CountConverted(
        Guid congregationId,
        VisitorFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(v => v.CongregationId == congregationId && v.ConvertedToMemberPersonId != null)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public void Create(Visitor entity) => _dbSet.Add(entity);

    public void SoftDelete(Visitor entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class VisitorQueryExtensions
{
    internal static IQueryable<Visitor> Filter(
        this IQueryable<Visitor> query,
        VisitorFilters filters,
        TimeProvider timeProvider
    )
    {
        if (filters.Name is not null)
            query = query.Where(v => EF.Functions.ILike(v.Person.Name, $"%{filters.Name}%"));

        if (filters.Converted.HasValue)
            query = filters.Converted.Value
                ? query.Where(v => v.ConvertedToMemberPersonId != null)
                : query.Where(v => v.ConvertedToMemberPersonId == null);

        var from = filters.From ?? DateUtils.GetFirstDayOfWeek(timeProvider);
        var to = filters.To ?? DateUtils.GetDateToday(timeProvider);

        query = query.Where(v =>
            DateOnly.FromDateTime(v.CreatedAt) >= from && DateOnly.FromDateTime(v.CreatedAt) <= to
        );

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
