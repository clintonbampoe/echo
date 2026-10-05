using Echo.Data;
using Echo.Domain.Members;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Members;

public class MemberService(
    MemberRepository repository,
    PersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IMemberMapper mapper,
    IEncoder encoder,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    ILogger<MemberService> logger
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
        MemberLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<MemberResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<MemberResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.get_by_id");
        activity?.SetTag("member.id", id);

        Member? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.member.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("member.found", false);
            MemberLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        MemberLog.Found(logger, congregationId, id);
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

        var (person, member) = mapper.ToEntity(dto);

        person.Id = idGenerator.Generate();
        person.CongregationId = congregationId;

        member.PersonId = person.Id;
        member.CongregationId = congregationId;
        member.Person = person;

        using (instrumentation.ActivitySource.StartActivity("svc.member.persist"))
        {
            personRepository.Create(person);
            repository.Create(member);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("member.id", member.PersonId);
        MemberLog.Created(logger, congregationId, member.PersonId);

        var res = mapper.ToDto(member);
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
            MemberLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.member.persist"))
        {
            mapper.Patch(dto, entity.Person, entity);
            await unitOfWork.CommitAsync(ct);
        }

        MemberLog.Updated(logger, congregationId, id);

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
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("member.found", false);
            MemberLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.member.persist"))
        {
            personRepository.SoftDelete(entity.Person);
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        MemberLog.Deleted(logger, congregationId, id);

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
        MemberLog.Searched(logger, congregationId, searchString, res.Count);
        return new SuccessResult<List<MemberSearchResultDto>>(res);
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        MemberFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.summary");

        var totalTask = repository.Count(congregationId, filters, ct);
        var activeTask = repository.CountActive(congregationId, filters, ct);
        var maleTask = repository.CountByGender(congregationId, Gender.Male, filters, ct);
        var femaleTask = repository.CountByGender(congregationId, Gender.Female, filters, ct);
        var avgAgeTask = repository.AverageAge(congregationId, filters, ct);

        await Task.WhenAll(totalTask, activeTask, maleTask, femaleTask, avgAgeTask);

        var res = new MemberSummaryDto
        {
            TotalMembers = totalTask.Result,
            ActiveMembers = activeTask.Result,
            MaleCount = maleTask.Result,
            FemaleCount = femaleTask.Result,
            AverageAge = avgAgeTask.Result,
        };

        activity?.SetTag("member.summary.total", res.TotalMembers);
        MemberLog.Summarized(logger, congregationId, res.TotalMembers);

        return new SuccessResult<MemberSummaryDto>(res);
    }

    private static MemberCursor BuildCursor(Member last) =>
        new MemberCursor { Name = last.Person.Name, Id = last.PersonId };
}
