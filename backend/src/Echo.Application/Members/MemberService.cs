using Echo.Data;
using Echo.Domain.Members;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Members;

public class MemberService(
    MemberRepository repository,
    IUnitOfWork unitOfWork,
    IMemberMapper mapper,
    IEncoder encoder,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        MemberFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var cursor = encoder.Decode<MemberCursor>(pagination.Cursor);

        var entities = await repository.List(
            congregationId,
            filters,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<MemberResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<MemberResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(id, congregationId, ct);
        if (entity is null)
            return new NotFoundResult(nameof(entity));

        var res = mapper.ToDto(entity);
        return new SuccessResult<MemberResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        MemberCreateDto dto,
        CancellationToken ct
    )
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<MemberResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        MemberUpdateDto dto,
        CancellationToken ct
    )
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<MemberResponseDto>(res);
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

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        var entities = await repository.Search(congregationId, searchString, ct);
        var res = mapper.ToSearchDto(entities);
        return new SuccessResult<List<MemberSearchResultDto>>(res);
    }

    public Task<IOperationResult> GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    private static MemberCursor BuildCursor(Member last)
    {
        return new MemberCursor { Name = last.Name, Id = last.Id };
    }
}
