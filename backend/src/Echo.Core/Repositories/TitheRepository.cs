using Echo.Application.Query.Extensions;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class TitheRepository(AppDbContext context)
{
    private readonly DbSet<Tithe> _dbSet = context.Set<Tithe>();

    public async Task<List<Tithe>> List(
        Guid congregationId,
        TitheFilter filters,
        TitheCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        var res = await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Include(t => t.Member)
            .Filter(filters)
            .OrderByDescending(t => t.CollectionDate)
            .ThenBy(t => t.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);

        return res;
    }

    public async Task<Tithe?> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Include(t => t.Member)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(Tithe entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Tithe entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class TitheQueryExtensions
{
    internal static IQueryable<Tithe> Filter(this IQueryable<Tithe> query, TitheFilter filters)
    {
        var currentYear = TimeProvider.System.GetUtcNow().Year;
        var currentMonth = (MonthOfYear)TimeProvider.System.GetUtcNow().Month;

        query = filters.Year is not null
            ? query.Where(t => t.ForYear == filters.Year)
            : query.Where(t => t.ForYear == currentYear);

        query = filters.Month is not null
            ? query.Where(t => t.ForMonth == filters.Month)
            : query.Where(t => t.ForMonth == currentMonth);

        if (filters.PaymentMethod is not null)
            query = query.Where(t => t.PaymentMethod == filters.PaymentMethod);

        if (filters.MemberId is not null)
            query = query.Where(t => t.MemberId == filters.MemberId);

        return query;
    }

    internal static IQueryable<Tithe> Paginate(
        this IQueryable<Tithe> query,
        TitheCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(t =>
                t.CollectionDate < cursor.CollectionDate
                || (t.CollectionDate == cursor.CollectionDate && t.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
