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

        var res = new PagedResponse<EventResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.event.get_by_id");
        activity?.SetTag("event.id", id);

        Event? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.event.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event.found", false);
            EventLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<EventResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        EventCreateDto dto,
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
            EventLog.OrganizerNotFound(logger, dto.OrganizerId);
            return new ForeignKeyEntityNotFound(nameof(organizer));
        }

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
            EventLog.OrganizationNotFound(logger, dto.OrganizationId);
            return new ForeignKeyEntityNotFound(nameof(organization));
        }

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
        EventLog.Created(logger, entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<EventResponseDto>(res);
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
            EventLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.event.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        EventLog.Updated(logger, id);

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
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("event.found", false);
            EventLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.event.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        EventLog.Deleted(logger, id);

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
        return new SuccessResult<List<EventSearchResultDto>>(res);
    }

    private static EventCursor BuildCursor(Event last)
    {
        return new EventCursor { StartDate = last.StartDate, Id = last.Id };
    }

    public Task<IOperationResult> GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
