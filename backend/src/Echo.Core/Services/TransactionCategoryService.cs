using AutoMapper;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;

namespace Echo.Core.Services;

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
