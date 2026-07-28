using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class TransactionRepository(AppDbContext context)
    : PrimaryRepositoryBase<Transaction>(context)
{
    public async Task<PagedResponse<TransactionListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplyDateFilters(queryParameters)
            .Where(t => t.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(t => t.Id)
            .Select(t => new TransactionListResponseDto
            {
                Id = t.Id,
                CategoryName = t.Category.Name,
                TransactionType = t.TransactionType,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<TransactionListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<TransactionResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                CategoryId = t.Category.Id,
                CategoryName = t.Category.Name,
                TransactionType = t.TransactionType,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<FinanceSummaryDto> GetSummaryAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateOnly(now.Year, now.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var totals = await DbSet
            .ApplySoftDeleteFilter()
            .Where(t =>
                t.CongregationId == congregationId && t.TransactionDate >= previousMonthStart
            )
            .GroupBy(t => new
            {
                t.TransactionType,
                IsCurrentMonth = t.TransactionDate >= currentMonthStart,
            })
            .Select(g => new
            {
                g.Key.TransactionType,
                g.Key.IsCurrentMonth,
                Total = g.Sum(t => t.Amount),
            })
            .ToListAsync(ct);

        decimal Get(TransactionType type, bool isCurrent) =>
            totals
                .FirstOrDefault(x => x.TransactionType == type && x.IsCurrentMonth == isCurrent)
                ?.Total
            ?? 0;

        var currentIncome = Get(TransactionType.Income, true);
        var currentExpense = Get(TransactionType.Expense, true);
        var previousIncome = Get(TransactionType.Income, false);
        var previousExpense = Get(TransactionType.Expense, false);

        return new FinanceSummaryDto
        {
            TotalIncome = currentIncome,
            IncomeDelta = currentIncome - previousIncome,
            TotalExpenditure = currentExpense,
            ExpenditureDelta = currentExpense - previousExpense,
            NetBalance = currentIncome - currentExpense,
            NetBalanceDelta = (currentIncome - currentExpense) - (previousIncome - previousExpense),
        };
    }

    public async Task<FinanceStreamsDto> GetStreamsAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var grouped = await DbSet
            .ApplySoftDeleteFilter()
            .Where(t => t.CongregationId == congregationId)
            .GroupBy(t => new
            {
                t.TransactionType,
                t.CategoryId,
                t.Category.Name,
            })
            .Select(g => new
            {
                g.Key.TransactionType,
                g.Key.CategoryId,
                g.Key.Name,
                Total = g.Sum(t => t.Amount),
            })
            .ToListAsync(ct);

        List<TransactionStreamDto> Build(TransactionType type)
        {
            var rows = grouped.Where(r => r.TransactionType == type).ToList();
            var total = rows.Sum(r => r.Total);

            return rows.Select(r => new TransactionStreamDto
                {
                    CategoryId = r.CategoryId,
                    CategoryName = r.Name,
                    Total = r.Total,
                    PercentOfTotal = total == 0 ? 0 : Math.Round(r.Total / total * 100, 1),
                })
                .ToList();
        }

        return new FinanceStreamsDto
        {
            IncomeStreams = Build(TransactionType.Income),
            ExpenditureStreams = Build(TransactionType.Expense),
        };
    }
}
