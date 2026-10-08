using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Echo.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Transactions;

public class TransactionRepository(AppDbContext context, TimeProvider timeProvider)
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

    public Task<decimal> SumIncome(
        Guid congregationId,
        TransactionFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t =>
                t.CongregationId == congregationId && t.TransactionType == TransactionType.Income
            )
            .Filter(filters, timeProvider)
            .SumAsync(t => t.Amount, ct);

    public Task<decimal> SumExpenses(
        Guid congregationId,
        TransactionFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t =>
                t.CongregationId == congregationId && t.TransactionType == TransactionType.Expense
            )
            .Filter(filters, timeProvider)
            .SumAsync(t => t.Amount, ct);

    public Task<string?> MostActiveCategory(
        Guid congregationId,
        TransactionFilters filters,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters, timeProvider)
            .GroupBy(t => t.Category.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync(ct);

    public void Create(Transaction entity) => _dbSet.Add(entity);

    public void SoftDelete(Transaction entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class TransactionQueryExtensions
{
    internal static IQueryable<Transaction> Filter(
        this IQueryable<Transaction> query,
        TransactionFilters filters,
        TimeProvider timeProvider
    )
    {
        if (filters.CategoryId is not null)
            query = query.Where(t => t.CategoryId == filters.CategoryId);

        if (filters.TransactionType is not null)
            query = query.Where(t => t.TransactionType == filters.TransactionType);

        var from = filters.From ?? DateUtils.GetFirstDayOfWeek(timeProvider);
        var to = filters.To ?? DateUtils.GetDateToday(timeProvider);

        query = query.Where(t => t.TransactionDate >= from && t.TransactionDate <= to);

        return query;
    }

    internal static IQueryable<Transaction> Paginate(
        this IQueryable<Transaction> query,
        TransactionCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(t =>
                t.TransactionDate < cursor.TransactionDate
                || (t.TransactionDate == cursor.TransactionDate && t.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
