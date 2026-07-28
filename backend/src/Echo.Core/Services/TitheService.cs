using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class TitheService(TitheRepository repository, AppDbContext context, IMapper mapper)
    : PrimaryServiceBase<Tithe>(repository, context, mapper)
{
    private readonly TitheRepository _titheRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _titheRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<TitheListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _titheRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Tithe not found.");

        return new SuccessResult<TitheResponseDto>(result);
    }
}
