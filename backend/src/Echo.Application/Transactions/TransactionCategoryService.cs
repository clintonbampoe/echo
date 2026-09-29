using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.HttpResults;

namespace Echo.Application.Transactions;

public class TransactionCategoryService(
    TransactionCategoryRepository repository,
    IUnitOfWork unitOfWork,
    ITransactionCategoryMapper mapper,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction_category.list"
        );

        List<TransactionCategory> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.fetch.all"))
        {
            entities = await repository.GetAll(congregationId, ct);
        }

        var res = mapper.ToListDto(entities);
        activity?.SetTag("transaction_category.count", res.Count);

        return new SuccessResult<List<TransactionCategoryResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(int id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction_category.get_by_id"
        );
        activity?.SetTag("transaction_category.id", id);

        TransactionCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction_category.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<TransactionCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        TransactionCategoryCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction_category.create"
        );

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("transaction_category.id", entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<TransactionCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        int id,
        TransactionCategoryUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction_category.update"
        );
        activity?.SetTag("transaction_category.id", id);

        TransactionCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction_category.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<TransactionCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction_category.delete"
        );
        activity?.SetTag("transaction_category.id", id);

        TransactionCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction_category.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction_category.search"
        );
        activity?.SetTag("transaction_category.query", name);

        List<TransactionCategory> entities;
        using (
            instrumentation.ActivitySource.StartActivity("svc.transaction_category.fetch.search")
        )
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("transaction_category.count", res.Count);
        return new SuccessResult<List<TransactionCategorySearchResponseDto>>(res);
    }
}
