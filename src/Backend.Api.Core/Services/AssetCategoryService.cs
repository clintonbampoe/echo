using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

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
