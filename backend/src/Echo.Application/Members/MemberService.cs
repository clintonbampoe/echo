using Echo.Data;
using Echo.Domain.Members;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Echo.Shared.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
    LinkGenerator linker,
    DateWindows dateWindows,
    ILogger<MemberService> logger
)
{
    internal static DateWindow? ResolveWindow(MemberFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisYear(filters.From, filters.To, hasParent: false);

    public async Task<IOperationResult> List(
        Guid congregationId,
        MemberFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

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

        var res = new PagedResponse<MemberResponseDto>(hasMore, nextCursor, data, window);
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
        HttpContext httpContext,
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

        var location =
            linker.GetPathByName(httpContext, "GetMemberById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetMemberById' is not registered.");
        return new CreatedResult<MemberResponseDto>(location, res);
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
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.member.summary");

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new MemberSummaryDto
        {
            Window = window,
            TotalMembers = stats.Total,
            ActiveMembers = stats.Active,
            MaleCount = stats.Male,
            FemaleCount = stats.Female,
            AverageAge = stats.AverageAge,
            AgeGroups = AgeGroupsOf(stats),
        };

        activity?.SetTag("member.summary.total", res.TotalMembers);
        MemberLog.Summarized(logger, congregationId, res.TotalMembers);

        return new SuccessResult<MemberSummaryDto>(res);
    }

    private static MemberCursor BuildCursor(Member last) =>
        new() { Name = last.Person.Name, Id = last.PersonId };

    // Always the same five groups in the same order, so a client can draw them without checking.
    private static AgeGroupDto[] AgeGroupsOf(MemberStats s) =>
        [
            new() { Label = "Under 18", Count = s.Under18 },
            new() { Label = "18–29", Count = s.Age18To29 },
            new() { Label = "30–44", Count = s.Age30To44 },
            new() { Label = "45–59", Count = s.Age45To59 },
            new() { Label = "60+", Count = s.Age60Plus },
        ];
}
