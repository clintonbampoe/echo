using Echo.Application.Caching;
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
    DateWindows dateWindows,
    SummaryCache summaries,
    ILogger<VisitorService> logger
)
{
    // Someone who was invited was, by definition, invited: naming who invited them fills the source
    // in, and it cannot say they walked in or came online.
    internal static (VisitorSource? Source, string? Problem) SourceFor(
        VisitorSource? requested,
        Guid? invitedBy
    )
    {
        return invitedBy is null ? (requested, null)
            : requested is null or VisitorSource.Invited ? (VisitorSource.Invited, null)
            : (
                requested,
                "A visitor who came by themselves or online cannot have someone who invited them."
            );
    }

    internal static DateWindow? ResolveWindow(VisitorFilters filters, DateWindows dateWindows)
    {
        return dateWindows.ThisYear(filters.From, filters.To, hasParent: false);
    }

    public async Task<IOperationResult> List(
        Guid congregationId,
        VisitorFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

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

        var stats = await repository.GetVisitStats(
            congregationId,
            [.. entities.Select(x => x.PersonId)],
            ct
        );

        var data = entities
            .Select(x => WithVisitStats(mapper.ToDto(x), x.PersonId, stats))
            .ToList();
        activity?.SetTag("visitor.count", data.Count);
        VisitorLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<VisitorResponseDto>(hasMore, nextCursor, data, window);
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
        var res = await WithVisitStats(entity, congregationId, ct);
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

        var (source, problem) = SourceFor(dto.Source, dto.InvitedByMemberId);
        if (problem is not null)
            return new BadRequestResult(problem);

        if (
            dto.InvitedByMemberId is { } inviter
            && await memberRepository.GetById(congregationId, inviter, ct) is null
        )
            return new ForeignKeyEntityNotFound("invitedBy");

        var (person, visitor) = mapper.ToEntity(dto);
        visitor.Source = source;

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

        if (visitor.ConvertedAt is not null)
        {
            VisitorLog.AlreadyConverted(logger, congregationId, visitorId);
            return new ConflictResult(visitorId.ToString());
        }

        visitor.Person.Kind = PersonKind.Member;
        var member = mapper.ToMemberEntity(dto);
        member.PersonId = visitor.PersonId;
        member.CongregationId = congregationId;
        member.Person = visitor.Person;

        var now = DateTime.UtcNow;
        visitor.ConvertedAt = now;
        member.JoinedDate = DateOnly.FromDateTime(now);

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

        var (source, problem) = SourceFor(
            dto.Source ?? entity.Source,
            dto.InvitedByMemberId ?? entity.InvitedByMemberId
        );
        if (problem is not null)
            return new BadRequestResult(problem);

        if (
            dto.InvitedByMemberId is { } inviter
            && await memberRepository.GetById(congregationId, inviter, ct) is null
        )
            return new ForeignKeyEntityNotFound("invitedBy");

        using (instrumentation.ActivitySource.StartActivity("svc.visitor.persist"))
        {
            mapper.Patch(dto, entity.Person, entity);
            entity.Source = source;
            await unitOfWork.CommitAsync(ct);
        }

        VisitorLog.Updated(logger, congregationId, id);

        var res = await WithVisitStats(entity, congregationId, ct);
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

    public Task<IOperationResult> Summary(
        Guid congregationId,
        VisitorFilters filters,
        CancellationToken ct
    )
    {
        return summaries.Serve(
            "visitors",
            congregationId,
            filters,
            token => ComputeSummary(congregationId, filters, token),
            ct
        );
    }

    private async Task<VisitorSummaryDto> ComputeSummary(
        Guid congregationId,
        VisitorFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.visitor.summary");

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new VisitorSummaryDto
        {
            Window = window,
            TotalVisitors = stats.Total,
            NewVisitors = stats.New,
            RecurringVisitors = stats.Recurring,
            ConvertedVisitors = stats.Converted,
            InvitedVisitors = stats.Invited,
            MaleVisitors = stats.Male,
            FemaleVisitors = stats.Female,
        };

        activity?.SetTag("visitor.summary.total", res.TotalVisitors);
        VisitorLog.Summarized(logger, congregationId, res.TotalVisitors);

        return res;
    }

    private static VisitorCursor BuildCursor(Visitor last)
    {
        return new() { Name = last.Person.Name, Id = last.PersonId };
    }

    private static VisitorResponseDto WithVisitStats(
        VisitorResponseDto dto,
        Guid personId,
        Dictionary<Guid, (int Count, DateOnly LastVisited)> stats
    )
    {
        return stats.TryGetValue(personId, out var visit)
            ? dto with
            {
                VisitCount = visit.Count,
                LastVisitedAt = visit.LastVisited,
            }
            : dto;
    }

    private async Task<VisitorResponseDto> WithVisitStats(
        Visitor entity,
        Guid congregationId,
        CancellationToken ct
    )
    {
        var stats = await repository.GetVisitStats(congregationId, [entity.PersonId], ct);
        return WithVisitStats(mapper.ToDto(entity), entity.PersonId, stats);
    }
}
