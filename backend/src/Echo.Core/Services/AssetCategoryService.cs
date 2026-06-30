using AutoMapper;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;

namespace Echo.Core.Services;

public class AssetCategoryService(
    AssetCategoryRepository repository,
    AppDbContext context,
    IMapper mapper
) : ReferenceServiceBase<AssetCategory>(repository, context, mapper)
{
    private readonly AssetCategoryRepository _assetCategoryRepository = repository;

    public override async Task<IOperationResult> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _assetCategoryRepository.GetAllAsync(congregationId, ct);
        return new SuccessResult<IEnumerable<AssetCategoryResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        var result = await _assetCategoryRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Asset category not found.");

        return new SuccessResult<AssetCategoryResponseDto>(result);
    }
}
