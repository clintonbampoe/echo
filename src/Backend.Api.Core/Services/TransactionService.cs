using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

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
        CancellationToken ct = default
    )
    {
        var result = await _transactionRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Transaction not found.");

        return new SuccessResult<TransactionResponseDto>(result);
    }
}
