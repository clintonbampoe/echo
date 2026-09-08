using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.MemberMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class MemberService(
    MemberRepository repository,
    IUnitOfWork unitOfWork,
    IMemberMapper mapper,
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
        return new SuccessResult<PagedResponse<MemberListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);
        if (result is null)
            return new NotFoundResult("Member not found.");

        return new SuccessResult<MemberResponseDto>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, MemberCreateDto dto, CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<MemberResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, Guid id, MemberUpdateDto dto, CancellationToken ct)
    {
        var entity = await repository.GetEntityById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<MemberResponseDto>(res);
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

    public async Task<IOperationResult> GetSummary(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetSummary(congregationId, ct);
        return new SuccessResult<MemberSummaryDto>(result);
    }

    public async Task<IOperationResult> SearchMembersByName(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        var results = await repository.SearchMembersByName(congregationId, searchString, ct);
        return new SuccessResult<List<MemberListResponseDto>>(results);
    }
}
