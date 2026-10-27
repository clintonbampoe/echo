using Echo.Application.Caching;
using Echo.Application.Members;
using Echo.Application.Organizations;
using Echo.Data;
using Echo.Domain.Events;
using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Echo.Shared.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Events;

public class EventService(
    EventRepository repository,
    OrganizationRepository organizationRepository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IEventMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    DateWindows dateWindows,
    SummaryCache summaries,
    ILogger<EventService> logger
)
{
    internal static DateWindow? ResolveWindow(EventFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisYear(filters.From, filters.To, hasParent: filters.OrganizerId is not null);

    public async Task<IOperationResult> List(
        Guid congregationId,
        EventListFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.list");

        var cursor = UsableCursor(encoder.Decode<EventCursor>(pagination.Cursor), filters.Order);

        List<Event> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.event.fetch.list"))
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

        var nextCursor = hasMore
            ? encoder.Encode(BuildCursor(entities.Last(), filters.Order))
            : null;

        var data = await WithCounts(entities, ct);
        activity?.SetTag("event.count", data.Count);
        EventLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<EventResponseDto>(hasMore, nextCursor, data, window);
        return new SuccessResult<PagedResponse<EventResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.get_by_id");
        activity?.SetTag("event.id", id);

        Event? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event.found", false);
            EventLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        EventLog.Found(logger, congregationId, id);
        var res = await WithCount(entity, ct);
        return new SuccessResult<EventResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        EventCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.create");

        Member? organizer;
        using (instrumentation.ActivitySource.StartActivity("svc.event.validate.organizer"))
        {
            organizer = await memberRepository.GetById(congregationId, dto.OrganizerId, ct);
        }
        if (organizer is null)
        {
            EventLog.CreateOrganizerNotFound(logger, congregationId, dto.OrganizerId);
            return new ForeignKeyEntityNotFound(nameof(organizer));
        }
        EventLog.CreateOrganizerFound(logger, congregationId, dto.OrganizerId);

        Organization? organization;
        using (instrumentation.ActivitySource.StartActivity("svc.event.validate.organization"))
        {
            organization = await organizationRepository.GetById(
                congregationId,
                dto.OrganizationId,
                ct
            );
        }
        if (organization is null)
        {
            EventLog.CreateOrganizationNotFound(logger, congregationId, dto.OrganizationId);
            return new ForeignKeyEntityNotFound(nameof(organization));
        }
        EventLog.CreateOrganizationFound(logger, congregationId, dto.OrganizationId);

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Organization = organization;
        entity.Organizer = organizer;

        using (instrumentation.ActivitySource.StartActivity("svc.event.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("event.id", entity.Id);
        EventLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);
        var location =
            linker.GetPathByName(httpContext, "GetEventById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetEventById' is not registered.");
        return new CreatedResult<EventResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        EventUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.update");
        activity?.SetTag("event.id", id);

        Event? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event.found", false);
            EventLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        // Validate the NEW organizer, not the existing one.
        if (dto.OrganizerId.HasValue && dto.OrganizerId.Value != entity.OrganizerId)
        {
            Member? organizer;
            using (instrumentation.ActivitySource.StartActivity("svc.event.validate.organizer"))
            {
                organizer = await memberRepository.GetById(
                    congregationId,
                    dto.OrganizerId.Value,
                    ct
                );
            }

            if (organizer is null)
            {
                EventLog.UpdateOrganizerNotFound(logger, congregationId, dto.OrganizerId.Value);
                return new ForeignKeyEntityNotFound(nameof(organizer));
            }
            EventLog.UpdateOrganizerFound(logger, congregationId, dto.OrganizerId.Value);
        }

        // Validate the NEW organization, not the existing one.
        if (dto.OrganizationId.HasValue && dto.OrganizationId.Value != entity.OrganizationId)
        {
            Organization? organization;
            using (instrumentation.ActivitySource.StartActivity("svc.event.validate.organization"))
            {
                organization = await organizationRepository.GetById(
                    congregationId,
                    dto.OrganizationId.Value,
                    ct
                );
            }

            if (organization is null)
            {
                EventLog.UpdateOrganizationNotFound(
                    logger,
                    congregationId,
                    dto.OrganizationId.Value
                );
                return new ForeignKeyEntityNotFound(nameof(organization));
            }
            EventLog.UpdateOrganizationFound(logger, congregationId, dto.OrganizationId.Value);
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.event.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        EventLog.Updated(logger, congregationId, id);
        var res = await WithCount(entity, ct);
        return new SuccessResult<EventResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.delete");
        activity?.SetTag("event.id", id);

        Event? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event.found", false);
            EventLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.event.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        EventLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.search");
        activity?.SetTag("event.query", name);

        List<Event> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.event.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("event.count", res.Count);
        EventLog.Searched(logger, congregationId, name, res.Count);
        return new SuccessResult<List<EventSearchResultDto>>(res);
    }

    public Task<IOperationResult> Summary(
        Guid congregationId,
        EventFilters filters,
        CancellationToken ct
    ) =>
        summaries.Serve(
            "events",
            congregationId,
            filters,
            token => ComputeSummary(congregationId, filters, token),
            ct
        );

    private async Task<EventSummaryDto> ComputeSummary(
        Guid congregationId,
        EventFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.summary");

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new EventSummaryDto
        {
            Window = window,
            TotalEvents = stats.Total,
            UpcomingEvents = stats.Upcoming,
            TotalRegistered = stats.Registrations,
            TotalAttended = stats.Attendees,
        };

        activity?.SetTag("event.summary.total", res.TotalEvents);
        EventLog.Summarized(logger, congregationId, res.TotalEvents);

        return res;
    }

    public Task<IOperationResult> SeriesSummary(
        Guid congregationId,
        EventSeriesFilters filters,
        CancellationToken ct
    ) =>
        summaries.Serve(
            "events.by-event",
            congregationId,
            filters,
            token => ComputeSeries(congregationId, filters, token),
            ct
        );

    private async Task<List<EventSeriesDto>> ComputeSeries(
        Guid congregationId,
        EventSeriesFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        var scope = (EventFilters)filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.series_summary");

        return await repository.GetSeries(congregationId, scope, filters.Limit, ct);
    }

    public async Task<IOperationResult> MonthlySummary(
        Guid congregationId,
        EventMonthlyFilters filters,
        CancellationToken ct
    )
    {
        if (MonthRange.Problem(filters.Months) is { } problem)
            return new BadRequestResult(problem);

        return await summaries.Serve(
            "events.monthly",
            congregationId,
            filters,
            token => ComputeMonthly(congregationId, filters, token),
            ct
        );
    }

    private async Task<List<EventMonthlyDto>> ComputeMonthly(
        Guid congregationId,
        EventMonthlyFilters filters,
        CancellationToken ct
    )
    {
        var months = dateWindows.ChartMonths(filters.Months, filters.To);
        var (from, to) = MonthRange.Bounds(months);
        var rows = await repository.GetMonthlyTotals(congregationId, filters, from, to, ct);

        return MonthRange.Fill(
            months,
            rows.Select(r => new EventMonthlyDto
            {
                Year = r.Year,
                Month = r.Month,
                EventCount = r.Events,
                RegistrationCount = r.Registrations,
                AttendanceCount = r.Attendance,
            }),
            m => new YearMonth(m.Year, m.Month),
            m => new EventMonthlyDto { Year = m.Year, Month = m.Month }
        );
    }

    private static EventCursor BuildCursor(Event last, SortDirection order)
    {
        return new EventCursor
        {
            StartDate = last.StartDate,
            Id = last.Id,
            Order = order,
        };
    }

    // A cursor from the other direction would skip or repeat events, so it is ignored (the list starts
    // again), the same as any malformed cursor.
    internal static EventCursor? UsableCursor(EventCursor? cursor, SortDirection order) =>
        cursor is not null && cursor.Order == order ? cursor : null;

    private async Task<EventResponseDto> WithCount(Event entity, CancellationToken ct) =>
        (await WithCounts([entity], ct))[0];

    private async Task<List<EventResponseDto>> WithCounts(
        IReadOnlyList<Event> entities,
        CancellationToken ct
    )
    {
        var ids = entities.Select(x => x.Id).ToList();
        var registered = await repository.GetRegistrationCounts(ids, ct);
        var attended = await repository.GetAttendanceCounts(ids, ct);

        return
        [
            .. entities.Select(x =>
                mapper.ToDto(x) with
                {
                    RegistrationCount = registered.GetValueOrDefault(x.Id),
                    AttendanceCount = attended.GetValueOrDefault(x.Id),
                }
            ),
        ];
    }
}
