using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
