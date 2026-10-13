using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Echo.Shared.Utilities;

namespace Echo.Application.Attendances;

public class SessionService(
    SessionRepository repository,
    SessionLabelRepository sessionLabelRepository,
    IUnitOfWork unitOfWork,
    ISessionMapper mapper,
    IEncoder encoder,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    DateWindows dateWindows,
    ILogger<SessionService> logger
)
{
    internal static DateWindow? ResolveWindow(SessionFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisWeek(
            filters.From,
            filters.To,
            hasParent: false
        );

    public async Task<IOperationResult> List(
        Guid congregationId,
        SessionFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.session.list");

        var cursor = encoder.Decode<SessionCursor>(pagination.Cursor);

        List<Session> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.session.fetch.list"))
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

        var ids = entities.Select(s => s.Id).ToList();
        var counts = await repository.GetAttendanceCounts(congregationId, ids, ct);

        var data = entities
            .Select(s => mapper.ToDto(s) with { AttendanceCount = counts.GetValueOrDefault(s.Id) })
            .ToList();

        activity?.SetTag("session.count", data.Count);
        SessionLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<SessionResponseDto>(hasMore, nextCursor, data, window);
        return new SuccessResult<PagedResponse<SessionResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.session.get_by_id");
        activity?.SetTag("session.id", id);

        Session? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("session.found", false);
            SessionLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        var counts = await repository.GetAttendanceCounts(congregationId, [id], ct);
        var res = mapper.ToDto(entity) with { AttendanceCount = counts.GetValueOrDefault(id) };

        SessionLog.Found(logger, congregationId, id);
        return new SuccessResult<SessionResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        SessionCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.session.create");

        SessionLabel? label;
        using (instrumentation.ActivitySource.StartActivity("svc.session.validate.label"))
        {
            label = await sessionLabelRepository.GetById(congregationId, dto.SessionLabelId, ct);
        }

        if (label is null)
        {
            SessionLog.CreateLabelNotFound(logger, congregationId, dto.SessionLabelId);
            return new ForeignKeyEntityNotFound(nameof(label));
        }

        var entity = mapper.ToEntity(dto);
        entity.Id = idGenerator.Generate();
        entity.CongregationId = congregationId;
        entity.SessionLabel = label;
        entity.Status = SessionStatus.Scheduled;
        entity.CreatedBy = null; // populated when user-scoped auth lands

        using (instrumentation.ActivitySource.StartActivity("svc.session.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("session.id", entity.Id);
        SessionLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity) with { AttendanceCount = 0 };
        var location =
            linker.GetPathByName(httpContext, "GetSessionById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetSessionById' is not registered.");
        return new CreatedResult<SessionResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        SessionUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.session.update");
        activity?.SetTag("session.id", id);

        Session? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("session.found", false);
            SessionLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.session.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        SessionLog.Updated(logger, congregationId, id);

        var counts = await repository.GetAttendanceCounts(congregationId, [id], ct);
        var res = mapper.ToDto(entity) with { AttendanceCount = counts.GetValueOrDefault(id) };
        return new SuccessResult<SessionResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.session.delete");
        activity?.SetTag("session.id", id);

        Session? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("session.found", false);
            SessionLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.session.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        SessionLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        SessionFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.session.summary");

        // Sequential — DbContext is not thread-safe and cannot serve concurrent queries.
        var total = await repository.Count(congregationId, filters, ct);
        var completed = await repository.CountByStatus(
            congregationId,
            SessionStatus.Completed,
            filters,
            ct
        );
        var cancelled = await repository.CountByStatus(
            congregationId,
            SessionStatus.Cancelled,
            filters,
            ct
        );

        // Average attendance is computed from the sessions in scope.
        var sessions = await repository.List(
            congregationId,
            filters,
            cursor: null,
            pageSize: int.MaxValue,
            ct
        );
        var ids = sessions.Select(s => s.Id).ToList();
        var counts = await repository.GetAttendanceCounts(congregationId, ids, ct);
        var totalAttendance = counts.Values.Sum();
        var average = sessions.Count == 0 ? 0.0 : (double)totalAttendance / sessions.Count;

        var res = new SessionSummaryDto
        {
            Window = window,
            TotalSessions = total,
            CompletedSessions = completed,
            CancelledSessions = cancelled,
            TotalAttendance = totalAttendance,
            AverageAttendance = average,
        };

        activity?.SetTag("session.summary.total", res.TotalSessions);
        SessionLog.Summarized(logger, congregationId, res.TotalSessions);

        return new SuccessResult<SessionSummaryDto>(res);
    }

    private static SessionCursor BuildCursor(Session last) =>
        new() { Date = last.Date, Id = last.Id };
}
