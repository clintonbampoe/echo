using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class TransactionService(
    TransactionRepository repository,
    AppDbContext context,
    IMapper mapper
) : PrimaryServiceBase<Transaction>(repository, context, mapper)
{
    private readonly TransactionRepository _transactionRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _transactionRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<TransactionListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _transactionRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Transaction not found.");

        return new SuccessResult<TransactionResponseDto>(result);
    }

    public async Task<IOperationResult> GetSummaryAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _transactionRepository.GetSummaryAsync(congregationId, ct);
        return new SuccessResult<FinanceSummaryDto>(result);
    }

    public async Task<IOperationResult> GetStreamsAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _transactionRepository.GetStreamsAsync(congregationId, ct);
        return new SuccessResult<FinanceStreamsDto>(result);
    }
}
