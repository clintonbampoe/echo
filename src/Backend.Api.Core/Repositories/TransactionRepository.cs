using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class TransactionRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<Transaction>(context, mapper)
{
    public async Task<List<TransactionStreamDto>> GetStreamsAsync()
    {
        var totalAmount = await _dbSet.AsNoTracking().SumAsync(t => t.Amount);

        var streams = await _dbSet
            .AsNoTracking()
            .GroupBy(t => new { t.Category.CategoryType, t.Category.Name })
            .Select(s => new TransactionStreamDto
            {
                Type = s.Key.CategoryType,
                Category = s.Key.Name,
                Amount = s.Sum(s => s.Amount),
            })
            .ToListAsync();

        return streams;
    }

    public override async Task<PagedResponse<Transaction>> GetPageAsync(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken cancellationToken = default
    )
    {
        var totalRecords = await _dbSet.AsNoTracking().CountAsync(cancellationToken);

        var records = await _dbSet
            .AsNoTracking()
            .Include(t => t.Category)
            .ApplyPagination(paginationParameters)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Transaction>(records, paginationParameters, totalRecords);
    }

    public override async Task<Transaction?> GetByIdAsync(Guid Id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.Id == Id)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(ct);
    }
}
