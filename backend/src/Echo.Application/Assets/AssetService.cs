using Echo.Data;
using Echo.Domain.Assets;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Assets;

public class AssetService(
    AssetRepository repository,
    AssetCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IAssetMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    ILogger<AssetService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        AssetFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.list");

        var cursor = encoder.Decode<AssetCursor>(pagination.Cursor);

        List<Asset> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.list"))
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
        activity?.SetTag("asset.count", data.Count);

        var res = new PagedResponse<AssetResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<AssetResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.get_by_id");
        activity?.SetTag("asset.id", id);

        Asset? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset.found", false);
            AssetLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<AssetResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AssetCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.create");

        AssetLog.Creating(logger, congregationId);

        var entity = mapper.ToEntity(dto);

        AssetCategory? category;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.validate.category"))
        {
            category = await categoryRepository.GetById(congregationId, entity.CategoryId, ct);
        }

        if (category is null)
        {
            AssetLog.CategoryNotFound(logger, entity.CategoryId);
            return new ForeignKeyEntityNotFound(nameof(category));
        }

        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Category = category;

        using (instrumentation.ActivitySource.StartActivity("svc.asset.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("asset.id", entity.Id);
        AssetLog.Created(logger, entity.Id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<AssetResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        AssetUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.update");
        activity?.SetTag("asset.id", id);
        Asset? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset.found", false);
            AssetLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.asset.persist"))
        {
            mapper.Patch(dto, entity);
            await unitOfWork.CommitAsync(ct);
        }

        AssetLog.Updated(logger, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<AssetResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.delete");
        activity?.SetTag("asset.id", id);

        Asset? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset.found", false);
            AssetLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.asset.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        AssetLog.Deleted(logger, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.search");
        activity?.SetTag("asset.query", name);

        List<Asset> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("asset.count", res.Count);

        return new SuccessResult<List<AssetSearchResultDto>>(res);
    }

    private static AssetCursor BuildCursor(Asset last)
    {
        return new AssetCursor { Name = last.Name, Id = last.Id };
    }
}
