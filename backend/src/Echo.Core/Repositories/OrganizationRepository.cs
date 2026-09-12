using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class OrganizationRepository(AppDbContext context)
{
    private readonly DbSet<Organization> _dbSet = context.Set<Organization>();

    public async Task<List<Organization>> List(
        Guid congregationId,
        OrganizationCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(o => o.CongregationId == congregationId)
            .OrderBy(o => o.Name)
            .ThenBy(o => o.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Organization?> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
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
            .FilterSoftDeleted()
            .Where(o => o.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(Organization entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Organization entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class OrganizationQueryExtensions
{
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

        return query;
    }
}
