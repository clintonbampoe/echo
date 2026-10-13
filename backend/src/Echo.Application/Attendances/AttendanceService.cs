using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Domain.Members;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Echo.Shared.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public class AttendanceService(
    AttendanceRepository repository,
    SessionRepository sessionRepository,
    PersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IAttendanceMapper mapper,
    IEncoder encoder,
    IIdGenerator idGenerator,
    DateWindows dateWindows,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<AttendanceService> logger
)
{
    internal static DateWindow? ResolveWindow(AttendanceFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisWeekToDate(
            filters.From,
            filters.To,
            hasParent: filters.SessionId is not null || filters.PersonId is not null
        );

    public async Task<IOperationResult> List(
        Guid congregationId,
        AttendanceFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.list");

        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        var cursor = encoder.Decode<AttendanceCursor>(pagination.Cursor);

        List<Attendance> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.list"))
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
        activity?.SetTag("attendance.count", data.Count);
        AttendanceLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<AttendanceResponseDto>(hasMore, nextCursor, data, window);
        return new SuccessResult<PagedResponse<AttendanceResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance.get_by_id"
        );
        activity?.SetTag("attendance.id", id);

        Attendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            AttendanceLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        AttendanceLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AttendanceCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.create");

        Session? session;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.validate.session"))
        {
            session = await sessionRepository.GetById(congregationId, dto.SessionId, ct);
        }
        if (session is null)
        {
            AttendanceLog.CreateSessionNotFound(logger, congregationId, dto.SessionId);
            return new ForeignKeyEntityNotFound(nameof(session));
        }

        Person? person;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.validate.person"))
        {
            person = await personRepository.GetById(congregationId, dto.PersonId, ct);
        }
        if (person is null)
        {
            AttendanceLog.CreatePersonNotFound(logger, congregationId, dto.PersonId);
            return new ForeignKeyEntityNotFound(nameof(person));
        }

        // Duplicate check — the unique index is the backstop, this returns a
        // friendly error before hitting the constraint.
        var exists = await repository.ExistsForSessionAndPerson(
            congregationId,
            dto.SessionId,
            dto.PersonId,
            ct
        );
        if (exists)
        {
            AttendanceLog.DuplicateRecord(logger, congregationId, dto.SessionId, dto.PersonId);
            return new ConflictResult(dto.PersonId.ToString());
        }

        var entity = mapper.ToEntity(dto);
        entity.Id = idGenerator.Generate();
        entity.CongregationId = congregationId;
        entity.Session = session;
        entity.Person = person;
        entity.PersonKindAtTime = person.Kind;
        entity.RecordedBy = null; // populated when user-scoped auth lands

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("attendance.id", entity.Id);
        AttendanceLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);
        var location =
            linker.GetPathByName(httpContext, "GetAttendanceById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetAttendanceById' is not registered.");
        return new CreatedResult<AttendanceResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        AttendanceUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.update");
        activity?.SetTag("attendance.id", id);

        Attendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            AttendanceLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        AttendanceLog.Updated(logger, congregationId, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.delete");
        activity?.SetTag("attendance.id", id);

        Attendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            AttendanceLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        AttendanceLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        AttendanceFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.summary");

        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new AttendanceSummaryDto
        {
            TotalPresent = stats.Total,
            MembersPresent = stats.Members,
            VisitorsPresent = stats.Visitors,
            FirstTimeVisitors = stats.FirstTimeVisitors,
            Window = window,
        };

        activity?.SetTag("attendance.summary.total", res.TotalPresent);
        AttendanceLog.Summarized(logger, congregationId, res.TotalPresent);

        return new SuccessResult<AttendanceSummaryDto>(res);
    }

    private static AttendanceCursor BuildCursor(Attendance last) =>
        new() { SessionDate = last.Session.Date, Id = last.Id };
}
