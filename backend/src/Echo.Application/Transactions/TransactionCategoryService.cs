using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Transactions;

public class TransactionCategoryService(
    TransactionCategoryRepository repository,
    IUnitOfWork unitOfWork,
    ITransactionCategoryMapper mapper,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<TransactionCategoryService> logger
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

        var counts = await repository.GetTransactionCounts(
            congregationId,
            entities.Select(x => x.Id).ToList(),
            ct
        );
        var res = entities
            .Select(x => mapper.ToDto(x) with { TransactionCount = counts.GetValueOrDefault(x.Id) })
            .ToList();
        TransactionCategoryLog.Listed(logger, congregationId, res.Count);
        activity?.SetTag("transaction_category.count", res.Count);

        return new SuccessResult<List<TransactionCategoryResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, int id, CancellationToken ct)
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
            TransactionCategoryLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        TransactionCategoryLog.Found(logger, congregationId, id);
        var res = await WithCount(entity, congregationId, ct);
        return new SuccessResult<TransactionCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        TransactionCategoryCreateDto dto,
        HttpContext httpContext,
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
        TransactionCategoryLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);

        var location =
            linker.GetPathByName(httpContext, "GetTransactionCategoryById", new { id = res.Id })
            ?? throw new InvalidOperationException(
                "Route 'GetTransactionCategoryById' is not registered."
            );
        return new CreatedResult<TransactionCategoryResponseDto>(location, res);
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
            TransactionCategoryLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        TransactionCategoryLog.Updated(logger, congregationId, id);

        var res = await WithCount(entity, congregationId, ct);
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
            TransactionCategoryLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        if (await repository.IsInUse(congregationId, id, ct))
        {
            activity?.SetTag("transaction_category.in_use", true);
            return new ConflictResult(
                "This category is still used by existing transactions. Move them to another category before deleting it."
            );
        }

        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        TransactionCategoryLog.Deleted(logger, congregationId, id);

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
        TransactionCategoryLog.Searched(logger, congregationId, name, res.Count);
        activity?.SetTag("transaction_category.count", res.Count);
        return new SuccessResult<List<TransactionCategorySearchResponseDto>>(res);
    }

    private async Task<TransactionCategoryResponseDto> WithCount(TransactionCategory entity, Guid congregationId, CancellationToken ct)
    {
        var counts = await repository.GetTransactionCounts(congregationId, [entity.Id], ct);
        return mapper.ToDto(entity) with { TransactionCount = counts.GetValueOrDefault(entity.Id) };
    }

    public async Task<IOperationResult> Reassign(
        Guid congregationId,
        int id,
        ReassignCategoryDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction_category.reassign"
        );
        activity?.SetTag("transaction_category.id", id);
        activity?.SetTag("transaction_category.target_id", dto.TargetCategoryId);

        var sameError = ReassignRules.Check(id, dto.TargetCategoryId);
        if (sameError is not null)
            return new BadRequestResult(sameError);

        var source = await repository.GetById(congregationId, id, ct);
        if (source is null)
            return new NotFoundResult(id.ToString());

        var target = await repository.GetById(congregationId, dto.TargetCategoryId, ct);
        if (target is null)
            return new ForeignKeyEntityNotFound(nameof(target));

        var typeError = ReassignRules.CheckTypes(source, target);
        if (typeError is not null)
            return new BadRequestResult(typeError);

        int moved;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction_category.reassign.persist"))
        {
            moved = await repository.Reassign(
                congregationId,
                source,
                target.Id,
                dto.DeleteSource,
                ct
            );
        }

        activity?.SetTag("transaction_category.moved", moved);
        return new SuccessResult<ReassignCategoryResponseDto>(
            new ReassignCategoryResponseDto { Moved = moved, SourceDeleted = dto.DeleteSource }
        );
    }
}
