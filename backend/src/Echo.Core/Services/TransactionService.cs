using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Services.Encoders;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.TransactionMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class TransactionService(
    TransactionRepository repository,
    TransactionCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    ITransactionMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        TransactionFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var cursor = encoder.Decode<TransactionCursor>(pagination.Cursor);
        var entities = await repository.List(
            congregationId,
            filters,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<TransactionResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<TransactionResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<TransactionResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        TransactionCreateDto dto,
        CancellationToken ct
    )
    {
        var transactionCategory = await categoryRepository.GetById(
            congregationId,
            dto.CategoryId,
            ct
        );
        if (transactionCategory is null)
            return new ForeignKeyEntityNotFound(nameof(transactionCategory));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Category = transactionCategory;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<TransactionResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        TransactionUpdateDto dto,
        CancellationToken ct
    )
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<TransactionResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        repository.SoftDelete(entity);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }

    private TransactionCursor BuildCursor(Transaction last)
    {
        return new TransactionCursor { TransactionDate = last.TransactionDate, Id = last.Id };
    }
}
