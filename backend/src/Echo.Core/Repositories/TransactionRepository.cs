using Echo.Application.Query.Extensions;
using Echo.Application.Utilities;
using Echo.Core.Dtos;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class TransactionRepository(AppDbContext context)
{
    private readonly DbSet<Transaction> _dbSet = context.Set<Transaction>();

    public async Task<List<Transaction>> List(
        Guid congregationId,
        TransactionFilters filters,
        TransactionCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Include(t => t.Category)
            .Filter(filters)
            .OrderByDescending(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Transaction?> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(ct);
    }

    public void Create(Transaction entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Transaction entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }
}

internal static class TransactionQueryExtensions
{
    internal static IQueryable<Transaction> Filter(
        this IQueryable<Transaction> query,
        TransactionFilters filters
    )
    {
        var firstDayOfWeek = DateUtils.GetFirstDayOfWeek(TimeProvider.System.GetUtcNow().DateTime);

        query = filters.Date is not null
            ? query.Where(t => t.TransactionDate == filters.Date)
            : query.Where(t => t.TransactionDate == firstDayOfWeek);

        if (filters.CategoryId is not null)
            query = query.Where(a => a.CategoryId == filters.CategoryId);

        if (filters.TransactionType is not null)
            query = query.Where(a => a.TransactionType == filters.TransactionType);

        return query;
    }

    internal static IQueryable<Transaction> Paginate(
        this IQueryable<Transaction> query,
        TransactionCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(e =>
                e.TransactionDate < cursor.TransactionDate
                || (e.TransactionDate == cursor.TransactionDate && e.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
