using Echo.Application.Caching;
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
    AttendanceRepository attendanceRepository,
    SessionLabelRepository sessionLabelRepository,
    IUnitOfWork unitOfWork,
    ISessionMapper mapper,
    IEncoder encoder,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    DateWindows dateWindows,
    SummaryCache summaries,
    ILogger<SessionService> logger
)
{
    // Scheduled and cancelled sessions have no attendance, so only completed ones count.
    internal static double AverageAttendance(int attendance, int completedSessions) =>
        completedSessions == 0 ? 0.0 : (double)attendance / completedSessions;

    // A session nobody was marked present at has no entry, so its counts stay at zero.
    internal static SessionResponseDto WithAttendance(
        SessionResponseDto dto,
        Dictionary<Guid, SessionAttendance> attendance
    ) =>
        attendance.TryGetValue(dto.Id, out var counts)
            ? dto with
            {
                AttendanceCount = counts.Total,
                MembersPresent = counts.Members,
                VisitorsPresent = counts.Visitors,
                FirstTimeVisitors = counts.FirstTime,
            }
            : dto;

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
        var attendance = await repository.GetAttendance(ids, ct);

        var data = entities.Select(s => WithAttendance(mapper.ToDto(s), attendance)).ToList();

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

        var attendance = await repository.GetAttendance([id], ct);
        var res = WithAttendance(mapper.ToDto(entity), attendance);

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

        var res = mapper.ToDto(entity);
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

        var attendance = await repository.GetAttendance([id], ct);
        var res = WithAttendance(mapper.ToDto(entity), attendance);
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
            var records = await attendanceRepository.ListForSession(id, ct);
            foreach (var record in records)
                attendanceRepository.SoftDelete(record);

            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
            await attendanceRepository.RefreshFirstVisits(records, ct);
        }

        SessionLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public Task<IOperationResult> Summary(
        Guid congregationId,
        SessionFilters filters,
        CancellationToken ct
    ) =>
        summaries.Serve(
            "sessions",
            congregationId,
            filters,
            token => ComputeSummary(congregationId, filters, token),
            ct
        );

    private async Task<SessionSummaryDto> ComputeSummary(
        Guid congregationId,
        SessionFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.session.summary");

        // Sequential — DbContext is not thread-safe and cannot serve concurrent queries.
        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new SessionSummaryDto
        {
            Window = window,
            TotalSessions = stats.Total,
            CompletedSessions = stats.Completed,
            CancelledSessions = stats.Cancelled,
            TotalAttendance = stats.TotalAttendance,
            AverageAttendance = AverageAttendance(stats.CompletedAttendance, stats.Completed),
        };

        activity?.SetTag("session.summary.total", res.TotalSessions);
        SessionLog.Summarized(logger, congregationId, res.TotalSessions);

        return res;
    }

    private static SessionCursor BuildCursor(Session last) =>
        new() { Date = last.Date, Id = last.Id };
}
