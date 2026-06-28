using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Dtos.Core;
using Api.Core.Entities;
using Api.Core.Entities.Core;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

public class TransactionCategoryService(
    TransactionCategoryRepository repository,
    AppDbContext context,
    IMapper mapper
) : ReferenceServiceBase<TransactionCategory>(repository, context, mapper)
{
    private readonly TransactionCategoryRepository _transactionCategoryRepository = repository;

    public override async Task<IOperationResult> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _transactionCategoryRepository.GetAllAsync(congregationId, ct);
        return new SuccessResult<IEnumerable<TransactionCategoryResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        var result = await _transactionCategoryRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Transaction category not found.");

        return new SuccessResult<TransactionCategoryResponseDto>(result);
    }
}
