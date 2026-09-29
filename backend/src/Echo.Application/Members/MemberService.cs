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
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        MemberFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.list");

        var cursor = encoder.Decode<MemberCursor>(pagination.Cursor);

        List<Member> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.member.fetch.list"))
        {
            entities = await repository.List(
                congregationId,
                filters,
                cursor,
                pagination.PageSize + 1,
                ct
            );
        }

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("member.count", data.Count);

        var res = new PagedResponse<MemberResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<MemberResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.get_by_id");
        activity?.SetTag("member.id", id);

        Member? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.member.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("member.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<MemberResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        MemberCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.create");

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        using (instrumentation.ActivitySource.StartActivity("svc.member.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("member.id", entity.Id);

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
        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.update");
        activity?.SetTag("member.id", id);

        Member? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.member.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("member.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.member.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<MemberResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.delete");
        activity?.SetTag("member.id", id);

        Member? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.member.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("member.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.member.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.search");
        activity?.SetTag("member.query", searchString);

        List<Member> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.member.fetch.search"))
        {
            entities = await repository.Search(congregationId, searchString, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("member.count", res.Count);
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
