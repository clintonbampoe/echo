using AutoMapper;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;
using Echo.Shared.Pagination;
using Echo.Shared.Query;

namespace Echo.Core.Services;

public class AssetService(AssetRepository repository, AppDbContext context, IMapper mapper)
    : PrimaryServiceBase<Asset>(repository, context, mapper)
{
    private readonly AssetRepository _assetRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _assetRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<AssetListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        var result = await _assetRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Asset not found.");

        return new SuccessResult<AssetResponseDto>(result);
    }
}
