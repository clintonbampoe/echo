using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

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
