using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Transactions;

public class TransactionRepository(AppDbContext context)
{
    private readonly DbSet<Transaction> _dbSet = context.Set<Transaction>();

    public async Task<List<Transaction>> List(
        Guid congregationId,
        TimeProvider timeProvider,
        TransactionFilters filters,
        TransactionCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Include(t => t.Category)
            .Filter(filters, timeProvider)
            .OrderByDescending(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<Transaction?> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
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
        TransactionFilters filters,
        TimeProvider timeProvider
    )
    {
        var firstDayOfWeek = DateUtils.GetFirstDayOfWeek(timeProvider.GetUtcNow().DateTime);

        if (filters.Date is not null)
        {
            query = query.Where(t => t.TransactionDate == filters.Date);
        }
        else
        {
            var weekEnd = firstDayOfWeek.AddDays(7);
            var weekStart = firstDayOfWeek;
            query = query.Where(t => t.TransactionDate >= weekStart && t.TransactionDate < weekEnd);
        }

        if (filters.CategoryId is not null)
            query = query.Where(t => t.CategoryId == filters.CategoryId);

        if (filters.TransactionType is not null)
            query = query.Where(t => t.TransactionType == filters.TransactionType);

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
