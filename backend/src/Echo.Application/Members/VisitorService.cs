using Echo.Data;
using Echo.Domain.Members;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Members;

public class VisitorService(
    VisitorRepository repository,
    MemberRepository memberRepository,
    PersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IVisitorMapper mapper,
    IMemberMapper memberMapper,
    IEncoder encoder,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<VisitorService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        VisitorFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.list");

        var cursor = encoder.Decode<VisitorCursor>(pagination.Cursor);

        List<Visitor> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.visitor.fetch.list"))
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
        activity?.SetTag("visitor.count", data.Count);
        VisitorLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<VisitorResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<VisitorResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.get_by_id");
        activity?.SetTag("visitor.id", id);

        Visitor? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.visitor.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("visitor.found", false);
            VisitorLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        VisitorLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<VisitorResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        VisitorCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.create");

        var (person, visitor) = mapper.ToEntity(dto);

        person.Id = idGenerator.Generate();
        person.CongregationId = congregationId;

        visitor.PersonId = person.Id;
        visitor.CongregationId = congregationId;
        visitor.Person = person;

        using (instrumentation.ActivitySource.StartActivity("svc.visitor.persist"))
        {
            personRepository.Create(person);
            repository.Create(visitor);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("visitor.id", visitor.PersonId);
        VisitorLog.Created(logger, congregationId, visitor.PersonId);

        var res = mapper.ToDto(visitor);
        var location =
            linker.GetPathByName(httpContext, "GetVisitorById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetVisitorById' is not registered.");
        return new CreatedResult<VisitorResponseDto>(location, res);
    }

    public async Task<IOperationResult> Convert(
        Guid congregationId,
        Guid visitorId,
        MemberCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.convert");
        activity?.SetTag("visitor.id", visitorId);

        Visitor? visitor;
        using (instrumentation.ActivitySource.StartActivity("svc.visitor.fetch.by_id"))
        {
            visitor = await repository.GetById(congregationId, visitorId, ct);
        }

        if (visitor is null)
        {
            activity?.SetTag("visitor.found", false);
            VisitorLog.ConvertNotFound(logger, congregationId, visitorId);
            return new NotFoundResult(visitorId.ToString());
        }

        if (visitor.ConvertedToMemberPersonId is not null)
        {
            VisitorLog.AlreadyConverted(logger, congregationId, visitorId);
            return new ConflictResult(visitorId.ToString());
        }

        visitor.Person.Kind = PersonKind.Member;

        var member = mapper.ToMemberEntity(dto);
        member.PersonId = visitor.PersonId;
        member.CongregationId = congregationId;
        member.Person = visitor.Person;

        visitor.ConvertedToMemberPersonId = visitor.PersonId;
        visitor.ConvertedAt = DateTime.UtcNow;

        using (instrumentation.ActivitySource.StartActivity("svc.visitor.persist"))
        {
            memberRepository.Create(member);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("member.id", member.PersonId);
        VisitorLog.Converted(logger, congregationId, visitorId, member.PersonId);

        // Build the response from the new Member so the client gets the full record
        // and a Location header pointing at it.
        var memberDto = memberMapper.ToDto(member);

        var location =
            linker.GetPathByName(httpContext, "GetMemberById", new { id = memberDto.Id })
            ?? throw new InvalidOperationException("Route 'GetMemberById' is not registered.");
        return new CreatedResult<MemberResponseDto>(location, memberDto);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        VisitorUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.update");
        activity?.SetTag("visitor.id", id);

        Visitor? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.visitor.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("visitor.found", false);
            VisitorLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.visitor.persist"))
        {
            mapper.Patch(dto, entity.Person, entity);
            await unitOfWork.CommitAsync(ct);
        }

        VisitorLog.Updated(logger, congregationId, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<VisitorResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.delete");
        activity?.SetTag("visitor.id", id);

        Visitor? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.visitor.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("visitor.found", false);
            VisitorLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.visitor.persist"))
        {
            personRepository.SoftDelete(entity.Person);
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        VisitorLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.search");
        activity?.SetTag("visitor.query", searchString);

        List<Visitor> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.visitor.fetch.search"))
        {
            entities = await repository.Search(congregationId, searchString, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("visitor.count", res.Count);
        VisitorLog.Searched(logger, congregationId, searchString, res.Count);
        return new SuccessResult<List<VisitorSearchResultDto>>(res);
    }

    public async Task<IOperationResult> Convert(
        Guid congregationId,
        Guid visitorId,
        MemberCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.convert");
        activity?.SetTag("visitor.id", visitorId);

        Visitor? visitor;
        using (instrumentation.ActivitySource.StartActivity("svc.visitor.fetch.by_id"))
        {
            visitor = await repository.GetById(congregationId, visitorId, ct);
        }

        if (visitor is null)
        {
            activity?.SetTag("visitor.found", false);
            VisitorLog.ConvertNotFound(logger, congregationId, visitorId);
            return new NotFoundResult(visitorId.ToString());
        }

        if (visitor.ConvertedToMemberPersonId is not null)
        {
            VisitorLog.AlreadyConverted(logger, congregationId, visitorId);
            return new ConflictResult(visitorId.ToString());
        }

        // Flip Person.Kind
        visitor.Person.Kind = PersonKind.Member;

        // Create Member side table row using existing Person
        var member = mapper.ToMemberEntity(dto);
        member.PersonId = visitor.PersonId;
        member.CongregationId = congregationId;
        member.Person = visitor.Person;

        // Link visitor to member
        visitor.ConvertedToMemberPersonId = visitor.PersonId;
        visitor.ConvertedAt = DateTime.UtcNow;

        using (instrumentation.ActivitySource.StartActivity("svc.visitor.persist"))
        {
            memberRepository.Create(member);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("member.id", member.PersonId);
        VisitorLog.Converted(logger, congregationId, visitorId, member.PersonId);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        VisitorFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.summary");

        var totalTask = repository.Count(congregationId, filters, ct);
        var newTask = repository.CountNew(congregationId, filters, ct);
        var recurringTask = repository.CountRecurring(congregationId, filters, ct);
        var convertedTask = repository.CountConverted(congregationId, filters, ct);

        await Task.WhenAll(totalTask, newTask, recurringTask, convertedTask);

        var res = new VisitorSummaryDto
        {
            TotalVisitors = totalTask.Result,
            NewVisitors = newTask.Result,
            RecurringVisitors = recurringTask.Result,
            ConvertedVisitors = convertedTask.Result,
        };

        activity?.SetTag("visitor.summary.total", res.TotalVisitors);
        VisitorLog.Summarized(logger, congregationId, res.TotalVisitors);

        return new SuccessResult<VisitorSummaryDto>(res);
    }

    private static VisitorCursor BuildCursor(Visitor last) =>
        new() { Name = last.Person.Name, Id = last.PersonId };
}
