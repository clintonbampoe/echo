using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.TitheMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class TitheService(
    TitheRepository repository,
    IUnitOfWork unitOfWork,
    ITitheMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetPage(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<TitheListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Tithe not found.");

        return new SuccessResult<TitheResponseDto>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, TitheCreateDto dto, CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, Guid id, TitheUpdateDto dto, CancellationToken ct)
    {
        var entity = await repository.GetEntityById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetEntityById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        await repository.SoftDelete(entity, ct);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }

    public async Task<IOperationResult> GetAnnualSummary(
        Guid congregationId,
        int year,
        CancellationToken ct = default
    )
    {
        if (year == 0)
            year = DateTime.UtcNow.Year;

        var result = await repository.GetAnnualSummary(congregationId, year, ct);
        return new SuccessResult<List<TitheMonthlyTotalDto>>(result);
    }
}
