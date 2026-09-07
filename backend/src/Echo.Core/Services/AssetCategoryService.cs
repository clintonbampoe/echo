using Echo.Application.HttpResults;
using Echo.Core.Dtos;
using Echo.Core.Mapping.AssetCategoryMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class AssetCategoryService(
    AssetCategoryRepository repository,
    IUnitOfWork unitOfWork,
    IAssetCategoryMapper mapper
)
{
    public async Task<IOperationResult> GetAll(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetAll(congregationId, ct);
        return new SuccessResult<IEnumerable<AssetCategoryResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        int id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Asset category not found.");

        return new SuccessResult<AssetCategoryResponseDto>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, AssetCategoryCreateDto dto,
        CancellationToken ct = default)
    {
        var entity = mapper.ToEntity(dto);

        entity.CongregationId = congregationId;

        await repository.Create(entity, ct);

        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);

        return new CreatedAtResult<AssetCategoryResponseDto>(res);
    }

    public Task<IOperationResult> Update(Guid congregationId, int id, AssetCategoryUpdateDto dto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
