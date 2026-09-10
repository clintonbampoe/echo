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
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    ITitheMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var entities = await repository.GetPage(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<TitheResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        TitheCreateDto dto,
        CancellationToken ct
    )
    {
        var member = await memberRepository.GetById(congregationId, dto.MemberId, ct);
        if (member is null)
            return new ForeignKeyEntityNotFound(nameof(member));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Member = member;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        TitheUpdateDto dto,
        CancellationToken ct
    )
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        repository.SoftDelete(entity);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }

    public Task<IOperationResult> GetSummary(Guid congregationId, int year, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
