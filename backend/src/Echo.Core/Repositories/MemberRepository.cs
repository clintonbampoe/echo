using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class MemberRepository(AppDbContext context)
{
    private readonly DbSet<Member> _dbSet = context.Set<Member>();

    public async Task<List<Member>> GetPage(
        Guid congregationId,
        MemberFilters filters,
        MemberCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(m => m.CongregationId == congregationId)
            .Filter(filters)
            .Paginate(cursor)
            .OrderBy(m => m.Name)
            .ThenBy(m => m.Id)
            .Take(pageSize + 1)
            .ToListAsync(ct);
    }

    public async Task<Member?> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(m => m.Id == id && m.CongregationId == congregationId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Member>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(m => m.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(Member entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Member entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task GetSummary(Guid congregationId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}

internal static class MemberQueryExtensions
{
    internal static IQueryable<Member> Filter(this IQueryable<Member> query, MemberFilters filters)
    {
        if (filters.Status.HasValue)
            query = query.Where(m => m.Status == filters.Status.Value);

        if (filters.Gender.HasValue)
            query = query.Where(m => m.Gender == filters.Gender.Value);

        if (filters.JoinedDate is not null)
            query = query.Where(m => m.JoinedDate >= filters.JoinedDate);

        if (filters.Name is not null)
            query = query.Where(m => EF.Functions.ILike(m.Name, $"%{filters.Name}%"));

        return query;
    }

    internal static IQueryable<Member> Paginate(this IQueryable<Member> query, MemberCursor? cursor)
    {
        if (cursor is not null)
            query = query.Where(m =>
                string.Compare(m.Name, cursor.Name) > 0
                || (string.Compare(m.Name, cursor.Name) == 0 && m.Id > cursor.Id)
            );

        return query;
    }
}
