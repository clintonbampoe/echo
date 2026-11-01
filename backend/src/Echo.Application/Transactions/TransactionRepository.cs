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
            .TotalsByCategory(context.Set<TransactionCategory>())
            .ToListAsync(ct);

    // Income and expenses added up, for comparing one window with another.
    public async Task<List<TypeTotal>> GetTypeTotals(
        Guid congregationId,
        TransactionFilters filters,
        CancellationToken ct = default
    ) =>
        await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(t => t.CongregationId == congregationId)
            .Filter(filters)
            .TotalsByType()
            .ToListAsync(ct);

    public void Create(Transaction entity) => _dbSet.Add(entity);

    public void SoftDelete(Transaction entity) => entity.DeletedAt = DateTime.UtcNow;
}

public record TypeTotal(TransactionType Type, decimal Total);

internal static class TransactionQueryExtensions
{
    internal static IQueryable<TypeTotal> TotalsByType(this IQueryable<Transaction> query) =>
        query.GroupBy(t => t.TransactionType).Select(g => new TypeTotal(g.Key, g.Sum(t => t.Amount)));

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

    // Groups first and looks the names up after, so only the few totals go through the join.
    internal static IQueryable<TransactionCategoryTotalDto> TotalsByCategory(
        this IQueryable<Transaction> query,
        IQueryable<TransactionCategory> categories
    ) =>
        query
            .GroupBy(t => new { t.CategoryId, t.TransactionType })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.TransactionType,
                Total = g.Sum(t => t.Amount),
                Count = g.Count(),
            })
            .Join(
                categories,
                total => total.CategoryId,
                category => category.Id,
                (total, category) =>
                    new TransactionCategoryTotalDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        Type = total.TransactionType,
                        Total = total.Total,
                        Count = total.Count,
                    }
            )
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

        if (filters.PaymentMethod is not null)
            query = query.Where(t => t.PaymentMethod == filters.PaymentMethod);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var pattern = $"%{Echo.Shared.Query.Extensions.EscapeLike(filters.Search.Trim())}%";
            query = query.Where(t =>
                (t.Description != null && EF.Functions.ILike(t.Description, pattern))
                || EF.Functions.ILike(t.Category.Name, pattern)
            );
        }

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
        // The leading <= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every newer row.
        if (cursor is not null)
            query = query.Where(t =>
                t.TransactionDate <= cursor.TransactionDate
                && (t.TransactionDate < cursor.TransactionDate || t.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
