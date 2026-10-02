using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Events;
using Echo.Domain.Members;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public class EventAttendanceService(
    EventAttendanceRepository repository,
    EventRepository eventRepository,
    MemberRepository memberRepository,
    IEncoder encoder,
    IUnitOfWork unitOfWork,
    IEventAttendanceMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    ILogger<EventAttendanceService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_attendance.list"
        );

        var cursor = encoder.Decode<EventAttendanceCursor>(pagination.Cursor);

        List<EventAttendance> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.fetch.list"))
        {
            entities = await repository.List(congregationId, cursor, pagination.PageSize + 1, ct);
        }

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("event_attendance.count", data.Count);
        EventAttendanceLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<EventAttendanceResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventAttendanceResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_attendance.get_by_id"
        );
        activity?.SetTag("event_attendance.id", id);

        EventAttendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event_attendance.found", false);
            EventAttendanceLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<EventAttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> ListByEventId(
        Guid congregationId,
        Guid eventId,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_attendance.list_by_event"
        );

        var cursor = encoder.Decode<EventAttendanceCursor>(pagination.Cursor);
        List<EventAttendance> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.fetch.by_event"))
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
        activity?.SetTag("event_attendance.count", data.Count);

        var res = new PagedResponse<EventAttendanceResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventAttendanceResponseDto>>(res);
    }

    public async Task<IOperationResult> ListByMemberId(
        Guid congregationId,
        Guid memberId,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_attendance.list_by_member"
        );

        var cursor = encoder.Decode<EventAttendanceCursor>(pagination.Cursor);
        List<EventAttendance> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.fetch.by_member"))
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
        activity?.SetTag("event_attendance.count", data.Count);

        var res = new PagedResponse<EventAttendanceResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventAttendanceResponseDto>>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        EventAttendanceCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_attendance.create"
        );

        Event? evnt;
        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.validate.event"))
        {
            evnt = await eventRepository.GetById(congregationId, dto.EventId, ct);
        }
        if (evnt is null)
        {
            EventAttendanceLog.CreateEventNotFound(logger, congregationId, dto.EventId);
            return new ForeignKeyEntityNotFound(nameof(evnt));
        }
        EventAttendanceLog.CreateEventFound(logger, dto.EventId, congregationId);

        Member? member;
        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.validate.member"))
        {
            member = await memberRepository.GetById(congregationId, dto.MemberId, ct);
        }
        if (member is null)
        {
            EventAttendanceLog.CreateMemberNotFound(logger, congregationId, dto.MemberId);
            return new ForeignKeyEntityNotFound(nameof(member));
        }
        EventAttendanceLog.CreateMemberFound(logger, congregationId, dto.MemberId);

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Event = evnt;
        entity.Member = member;

        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("event_attendance.id", entity.Id);
        EventAttendanceLog.Created(logger, congregationId, entity.Id);
        var res = mapper.ToDto(entity);
        return new CreatedAtResult<EventAttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        EventAttendanceUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_attendance.update"
        );
        activity?.SetTag("event_attendance.id", id);

        EventAttendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event_attendance.found", false);
            EventAttendanceLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        EventAttendanceLog.Found(logger, congregationId, id);
        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        EventAttendanceLog.Updated(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<EventAttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.event_attendance.delete"
        );
        activity?.SetTag("event_attendance.id", id);

        EventAttendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event_attendance.found", false);
            EventAttendanceLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.event_attendance.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        EventAttendanceLog.Deleted(logger, congregationId, id);
        return new NoContentResult();
    }

    private static EventAttendanceCursor BuildCursor(EventAttendance last)
    {
        return new EventAttendanceCursor { CheckInTime = last.CheckInTime, Id = last.Id };
    }
}
