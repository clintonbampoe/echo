using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Api.Core.Dtos.Core;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

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
            .Select(t => new TransactionListResponseDto(
                t.Id,
                t.Category.Name,
                t.TransactionType,
                t.TransactionDate,
                t.Amount
            ))
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
            .Select(t => new TransactionResponseDto(
                t.Id,
                t.CategoryId,
                t.Category.Name,
                t.TransactionType,
                t.TransactionDate,
                t.Amount,
                t.Description,
                t.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
