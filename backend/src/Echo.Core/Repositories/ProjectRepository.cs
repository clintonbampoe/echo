using Echo.Application.Query.Extensions;
using Echo.Application.Utilities;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class ProjectRepository(AppDbContext context)
{
    private readonly DbSet<Project> _dbSet = context.Set<Project>();

    public async Task<List<Project>> List(
        Guid congregationId,
        ProjectFilters filters,
        ProjectCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(p => p.CongregationId == congregationId)
            .Include(p => p.Category)
            .Include(p => p.Manager)
            .Filter(filters)
            .OrderBy(p => p.StartDate)
            .ThenBy(p => p.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Project?> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .Include(p => p.Category)
            .Include(p => p.Manager)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Project>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(pr => pr.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(Project entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Project entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class ProjectQueryExtensions
{
    internal static IQueryable<Project> Filter(
        this IQueryable<Project> query,
        ProjectFilters filters
    )
    {
        var today = DateUtils.GetDateToday();

        query = filters.StartDate is not null
            ? query.Where(p => p.StartDate == filters.StartDate)
            : query.Where(p => p.StartDate == today);

        if (filters.CategoryId is not null)
            query = query.Where(p => p.CategoryId == filters.CategoryId);

        if (filters.Status is not null)
            query = query.Where(p => p.Status == filters.Status);

        if (filters.Name is not null)
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{filters.Name}%"));

        return query;
    }

    internal static IQueryable<Project> Paginate(
        this IQueryable<Project> query,
        ProjectCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(p =>
                p.StartDate > cursor.StartDate
                || (p.StartDate == cursor.StartDate && p.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
