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
    ILogger<EventService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        EventFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.list");

        var cursor = encoder.Decode<EventCursor>(pagination.Cursor);

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

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("event.count", data.Count);
        EventLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<EventResponseDto>(hasMore, nextCursor, data);
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
        var res = mapper.ToDto(entity);
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
        var res = mapper.ToDto(entity);
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

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        EventFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.summary");

        var countTask = repository.Count(congregationId, filters, ct);
        var upcomingTask = repository.CountUpcoming(congregationId, filters, ct);
        var registrationsTask = repository.CountRegistrations(congregationId, filters, ct);
        var attendeesTask = repository.CountAttendees(congregationId, filters, ct);

        await Task.WhenAll(countTask, upcomingTask, registrationsTask, attendeesTask);

        var res = new EventSummaryDto
        {
            TotalEvents = countTask.Result,
            UpcomingEvents = upcomingTask.Result,
            TotalRegistered = registrationsTask.Result,
            TotalAttended = attendeesTask.Result,
        };

        activity?.SetTag("event.summary.total", res.TotalEvents);
        EventLog.Summarized(logger, congregationId, res.TotalEvents);

        return new SuccessResult<EventSummaryDto>(res);
    }

    private static EventCursor BuildCursor(Event last)
    {
        return new EventCursor { StartDate = last.StartDate, Id = last.Id };
    }
}
