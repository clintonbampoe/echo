using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Events;
using Echo.Domain.Members;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public class EventRegistrationService(
    EventRegistrationRepository repository,
    EventRepository eventRepository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IEventRegistrationMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<EventRegistrationService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        PaginationRequest pagination,
        CancellationToken ct = default
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_registration.list"
        );

        var cursor = encoder.Decode<EventRegistrationCursor>(pagination.Cursor);
        List<EventRegistration> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.event_registration.fetch.list"))
        {
            entities = await repository.List(congregationId, cursor, pagination.PageSize + 1, ct);
        }

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("event_registration.count", data.Count);

        var res = new PagedResponse<EventRegistrationResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventRegistrationResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_registration.get_by_id"
        );
        activity?.SetTag("event_registration.id", id);

        EventRegistration? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event_registration.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event_registration.found", false);
            EventRegistrationLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<EventRegistrationResponseDto>(res);
    }

    public async Task<IOperationResult> ListByEventId(
        Guid congregationId,
        Guid eventId,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_registration.list_by_event"
        );

        var cursor = encoder.Decode<EventRegistrationCursor>(pagination.Cursor);
        List<EventRegistration> entities;
        using (
            instrumentation.ActivitySource.StartActivity("svc.event_registration.fetch.by_event")
        )
        {
            entities = await repository.ListByEventId(
                congregationId,
                eventId,
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
        activity?.SetTag("event_registration.count", data.Count);

        var res = new PagedResponse<EventRegistrationResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventRegistrationResponseDto>>(res);
    }

    public async Task<IOperationResult> ListByMemberId(
        Guid congregationId,
        Guid memberId,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_registration.list_by_member"
        );

        var cursor = encoder.Decode<EventRegistrationCursor>(pagination.Cursor);

        List<EventRegistration> entities;
        using (
            instrumentation.ActivitySource.StartActivity("svc.event_registration.fetch.by_member")
        )
        {
            entities = await repository.ListByMemberId(
                congregationId,
                memberId,
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
        activity?.SetTag("event_registration.count", data.Count);

        var res = new PagedResponse<EventRegistrationResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventRegistrationResponseDto>>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        EventRegistrationCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_registration.create"
        );

        Event? evnt;
        using (
            instrumentation.ActivitySource.StartActivity("svc.event_registration.validate.event")
        )
        {
            evnt = await eventRepository.GetById(congregationId, dto.EventId, ct);
        }
        if (evnt is null)
        {
            EventRegistrationLog.CreateEventNotFound(logger, congregationId, dto.EventId);
            return new ForeignKeyEntityNotFound(nameof(evnt));
        }
        EventRegistrationLog.CreateEventFound(logger, congregationId, dto.EventId);

        Member? member;
        using (
            instrumentation.ActivitySource.StartActivity("svc.event_registration.validate.member")
        )
        {
            member = await memberRepository.GetById(congregationId, dto.MemberId, ct);
        }
        if (member is null)
        {
            EventRegistrationLog.CreateMemberNotFound(logger, congregationId, dto.MemberId);
            return new ForeignKeyEntityNotFound(nameof(member));
        }
        EventRegistrationLog.CreateMemberFound(logger, congregationId, dto.MemberId);

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Event = evnt;
        entity.Member = member;

        using (instrumentation.ActivitySource.StartActivity("svc.event_registration.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("event_registration.id", entity.Id);
        EventRegistrationLog.Created(logger, congregationId, entity.Id);
        var res = mapper.ToDto(entity);

        var location =
            linker.GetPathByName(httpContext, "GetEventRegistrationById", new { id = res.Id })
            ?? throw new InvalidOperationException(
                "Route 'GetEventRegistrationById' is not registered."
            );
        return new CreatedResult<EventRegistrationResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        EventRegistrationUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_registration.update"
        );

        activity?.SetTag("event_registration.id", id);
        EventRegistration? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event_registration.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event_registration.found", false);
            EventRegistrationLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);
        using (instrumentation.ActivitySource.StartActivity("svc.event_registration.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        EventRegistrationLog.Updated(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<EventRegistrationResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_registration.delete"
        );

        activity?.SetTag("event_registration.id", id);
        EventRegistration? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event_registration.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event_registration.found", false);
            EventRegistrationLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.event_registration.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        EventRegistrationLog.Deleted(logger, congregationId, id);
        return new NoContentResult();
    }

    private static EventRegistrationCursor BuildCursor(EventRegistration last)
    {
        return new EventRegistrationCursor
        {
            RegistrationDate = last.RegistrationDate,
            Id = last.Id,
        };
    }
}
