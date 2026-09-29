using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Transactions;

public class TransactionService(
    TransactionRepository repository,
    TransactionCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    ITransactionMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        TransactionFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.transaction.list");

        var cursor = encoder.Decode<TransactionCursor>(pagination.Cursor);

        List<Transaction> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.fetch.list"))
        {
            entities = await repository.List(
                congregationId,
                filters,
                cursor,
                pagination.PageSize + 1,
                ct
            );
        }

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("transaction.count", data.Count);

        var res = new PagedResponse<TransactionResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<TransactionResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction.get_by_id"
        );
        activity?.SetTag("transaction.id", id);

        Transaction? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<TransactionResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        TransactionCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.transaction.create");

        TransactionCategory? transactionCategory;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.validate.category"))
        {
            transactionCategory = await categoryRepository.GetById(
                congregationId,
                dto.CategoryId,
                ct
            );
        }
        if (transactionCategory is null)
            return new ForeignKeyEntityNotFound(nameof(transactionCategory));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Category = transactionCategory;

        using (instrumentation.ActivitySource.StartActivity("svc.transaction.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("transaction.id", entity.Id);

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
        using var activity = instrumentation.ActivitySource.StartActivity("svc.transaction.update");
        activity?.SetTag("transaction.id", id);

        Transaction? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.transaction.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<TransactionResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.transaction.delete");
        activity?.SetTag("transaction.id", id);

        Transaction? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.transaction.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    private static TransactionCursor BuildCursor(Transaction last)
    {
        return new TransactionCursor { TransactionDate = last.TransactionDate, Id = last.Id };
    }
}
