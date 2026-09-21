using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Events;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Events;

public class EventRegistrationService(
    EventRegistrationRepository repository,
    EventRepository eventRepository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IEventRegistrationMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        PaginationRequest pagination,
        CancellationToken ct = default
    )
    {
        var cursor = encoder.Decode<EventRegistrationCursor>(pagination.Cursor);
        var entities = await repository.List(congregationId, cursor, pagination.PageSize + 1, ct);

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<EventRegistrationResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventRegistrationResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(id, congregationId, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

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
        var cursor = encoder.Decode<EventRegistrationCursor>(pagination.Cursor);
        var entities = await repository.ListByEventId(
            congregationId,
            eventId,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
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
        var cursor = encoder.Decode<EventRegistrationCursor>(pagination.Cursor);
        var entities = await repository.ListByMemberId(
            congregationId,
            memberId,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<EventRegistrationResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<EventRegistrationResponseDto>>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        EventRegistrationCreateDto dto,
        CancellationToken ct
    )
    {
        var evnt = await eventRepository.GetById(congregationId, dto.EventId, ct);
        if (evnt is null)
            return new ForeignKeyEntityNotFound(nameof(evnt));

        var member = await memberRepository.GetById(congregationId, dto.MemberId, ct);
        if (member is null)
            return new ForeignKeyEntityNotFound(nameof(member));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Event = evnt;
        entity.Member = member;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<EventRegistrationResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        EventRegistrationUpdateDto dto,
        CancellationToken ct
    )
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<EventRegistrationResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        repository.SoftDelete(entity);
        await unitOfWork.CommitAsync(ct);

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
