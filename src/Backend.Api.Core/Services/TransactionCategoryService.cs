using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

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
