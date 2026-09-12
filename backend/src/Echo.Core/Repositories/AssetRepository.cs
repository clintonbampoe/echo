using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AssetRepository(AppDbContext context)
{
    private readonly DbSet<Asset> _dbSet = context.Set<Asset>();

    public async Task<List<Asset>> List(
        Guid congregationId,
        AssetFilters filters,
        AssetCursor? cursor,
        int pageSize,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(a => a.CongregationId == congregationId)
            .Include(a => a.Category)
            .Filter(filters)
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Asset?> GetById(Guid id, Guid congregationId, CancellationToken ct = default)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(a => a.Id == id && a.CongregationId == congregationId)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Asset>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(a => a.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(Asset entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Asset entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class AssetQueryExtensions
{
    internal static IQueryable<Asset> Filter(this IQueryable<Asset> query, AssetFilters filters)
    {
        if (filters.Name is not null)
            query = query.Where(a => EF.Functions.ILike(a.Name, $"%{filters.Name}%"));

        if (filters.CategoryId is not null)
            query = query.Where(a => a.CategoryId == filters.CategoryId);

        if (filters.Status is not null)
            query = query.Where(a => a.Status == filters.Status);

        return query;
    }

    internal static IQueryable<Asset> Paginate(
        this IQueryable<Asset> query,
        AssetCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(a =>
                string.Compare(a.Name, cursor.Name) > 0
                || (a.Name == cursor.Name && a.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
