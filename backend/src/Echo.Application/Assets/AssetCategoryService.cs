using Echo.Data;
using Echo.Domain.Assets;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Assets;

public class AssetCategoryService(
    AssetCategoryRepository repository,
    IUnitOfWork unitOfWork,
    IAssetCategoryMapper mapper,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<AssetCategoryService> logger
)
{
    public async Task<IOperationResult> List(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.asset_category.list"
        );

        List<AssetCategory> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.asset_category.fetch.all"))
        {
            entities = await repository.GetAll(congregationId, ct);
        }

        var res = mapper.ToListDto(entities);
        AssetCategoryLog.Listed(logger, congregationId, res.Count);
        activity?.SetTag("asset_category.count", res.Count);

        return new SuccessResult<List<AssetCategoryResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.asset_category.get_by_id"
        );
        activity?.SetTag("asset_category.id", id);

        AssetCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset_category.found", false);
            AssetCategoryLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        AssetCategoryLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<AssetCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AssetCategoryCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.asset_category.create"
        );

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        using (instrumentation.ActivitySource.StartActivity("svc.asset_category.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("asset_category.id", entity.Id);
        AssetCategoryLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);
        var location =
            linker.GetPathByName(httpContext, "GetAssetCategoryById", new { id = res.Id })
            ?? throw new InvalidOperationException(
                "Route 'GetAssetCategoryById' is not registered."
            );
        return new CreatedResult<AssetCategoryResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        int id,
        AssetCategoryUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.asset_category.update"
        );
        activity?.SetTag("asset_category.id", id);

        AssetCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset_category.found", false);
            AssetCategoryLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        AssetCategoryLog.Found(logger, congregationId, id);
        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.asset_category.persist"))
        {
            await unitOfWork.CommitAsync(ct);
            AssetCategoryLog.Updated(logger, congregationId, id);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<AssetCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.asset_category.delete"
        );
        activity?.SetTag("asset_category.id", id);

        AssetCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset_category.found", false);
            AssetCategoryLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        AssetCategoryLog.Found(logger, congregationId, id);
        using (instrumentation.ActivitySource.StartActivity("svc.asset_category.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
            AssetCategoryLog.Deleted(logger, congregationId, id);
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
            "svc.asset_category.search"
        );
        activity?.SetTag("asset_category.query", name);

        List<AssetCategory> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.asset_category.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
            AssetCategoryLog.Searched(logger, congregationId, name, entities.Count);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("asset_category.count", res.Count);
        return new SuccessResult<List<AssetCategorySearchResultDto>>(res);
    }
}
