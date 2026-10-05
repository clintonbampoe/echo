using Echo.Data;
using Echo.Domain.Members;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Members;

public class MemberRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<Member> _dbSet = context.Set<Member>();

    public async Task<List<Member>> List(
        Guid congregationId,
        MemberFilters filters,
        MemberCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(m => m.CongregationId == congregationId)
            .Include(m => m.Person)
            .Filter(filters, timeProvider)
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

    public Task<int> Count(
        Guid congregationId,
        MemberFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(m => m.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<int> CountActive(
        Guid congregationId,
        MemberFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(m => m.CongregationId == congregationId && m.Status == MemberStatus.Active)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<int> CountByGender(
        Guid congregationId,
        Gender gender,
        MemberFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(m => m.CongregationId == congregationId && m.Gender == gender)
            .Filter(filters, timeProvider)
            .CountAsync(ct);

    public Task<double> AverageAge(
        Guid congregationId,
        MemberFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(m => m.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .AverageAsync(m => DateUtils.GetDateToday(timeProvider).Year - m.DateOfBirth.Year, ct);

    public void Create(Member entity) => _dbSet.Add(entity);

    public void SoftDelete(Member entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class MemberQueryExtensions
{
    internal static IQueryable<Member> Filter(
        this IQueryable<Member> query,
        MemberFilters filters,
        TimeProvider timeProvider
    )
    {
        if (filters.Name is not null)
            query = query.Where(m => EF.Functions.ILike(m.Person.Name, $"%{filters.Name}%"));

        if (filters.Status.HasValue)
            query = query.Where(m => m.Status == filters.Status.Value);

        if (filters.Gender.HasValue)
            query = query.Where(m => m.Gender == filters.Gender.Value);

        if (filters.Region.HasValue)
            query = query.Where(m => m.Region == filters.Region.Value);

        if (filters.MaritalStatus.HasValue)
            query = query.Where(m => m.MaritalStatus == filters.MaritalStatus.Value);

        var from = filters.From ?? DateUtils.GetFirstDayOfYear(timeProvider);
        var to = filters.To ?? DateUtils.GetLastDayOfYear(timeProvider);

        query = query.Where(m => m.JoinedDate >= from && m.JoinedDate <= to);

        return query;
    }

    internal static IQueryable<Member> Paginate(
        this IQueryable<Member> query,
        MemberCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(m =>
                string.Compare(m.Person.Name, cursor.Name) > 0
                || (m.Person.Name == cursor.Name && m.PersonId > cursor.Id)
            );

        return query.Take(pageSize);
    }

    internal static IQueryable<Member> SearchName(this IQueryable<Member> query, string name)
    {
        return query
            .Where(x => EF.Functions.ILike(x.Person.Name, $"%{name}%"))
            .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.Person.Name, name))
            .Take(5);
    }
}
