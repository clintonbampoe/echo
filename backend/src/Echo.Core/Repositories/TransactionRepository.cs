using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
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
        var query = _dbSet
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
                CategoryName =  t.Category.Name,
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

    public async Task<TransactionResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(t => t.Id == id)
            .Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                CategoryId =  t.Category.Id,
                CategoryName = t.Category.Name,
                TransactionType = t.TransactionType,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
                Description =  t.Description,
                CreatedAt =  t.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
