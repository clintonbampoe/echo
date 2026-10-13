using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Transactions;

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
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Include(t => t.Category)
            .Filter(filters)
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

    public async Task<List<TransactionMonthlyDto>> GetMonthlyTotals(
        Guid congregationId,
        TransactionFilters filters,
        CancellationToken ct = default
    ) =>
        await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters)
            .MonthlyTotals()
            .ToListAsync(ct);

    public async Task<List<TransactionCategoryTotalDto>> GetCategoryTotals(
        Guid congregationId,
        TransactionFilters filters,
        CancellationToken ct = default
    ) =>
        await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters)
            .TotalsByCategory()
            .ToListAsync(ct);

    public void Create(Transaction entity) => _dbSet.Add(entity);

    public void SoftDelete(Transaction entity) => entity.DeletedAt = DateTime.UtcNow;
}

internal static class TransactionQueryExtensions
{
    internal static IQueryable<TransactionMonthlyDto> MonthlyTotals(
        this IQueryable<Transaction> query
    ) =>
        query
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new TransactionMonthlyDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalIncome = g.Where(t => t.TransactionType == TransactionType.Income)
                    .Sum(t => t.Amount),
                TotalExpenses = g.Where(t => t.TransactionType == TransactionType.Expense)
                    .Sum(t => t.Amount),
            });

    internal static IQueryable<TransactionCategoryTotalDto> TotalsByCategory(
        this IQueryable<Transaction> query
    ) =>
        query
            .GroupBy(t => new
            {
                t.CategoryId,
                t.Category.Name,
                t.TransactionType,
            })
            .Select(g => new TransactionCategoryTotalDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Type = g.Key.TransactionType,
                Total = g.Sum(t => t.Amount),
                Count = g.Count(),
            })
            .OrderByDescending(c => c.Total)
            .ThenBy(c => c.CategoryName);

    internal static IQueryable<Transaction> Filter(
        this IQueryable<Transaction> query,
        TransactionFilters filters
    )
    {
        if (filters.CategoryId is not null)
            query = query.Where(t => t.CategoryId == filters.CategoryId);

        if (filters.TransactionType is not null)
            query = query.Where(t => t.TransactionType == filters.TransactionType);

        if (filters.From is not null)
            query = query.Where(t => t.TransactionDate >= filters.From);

        if (filters.To is not null)
            query = query.Where(t => t.TransactionDate <= filters.To);

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
