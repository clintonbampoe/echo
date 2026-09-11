using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.EventRegistrationMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class EventRegistrationService(
    EventRegistrationRepository repository,
    EventRepository eventRepository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEventRegistrationMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        Parameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var entities = await repository.GetAll(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<EventRegistrationResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(id, congregationId, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<EventRegistrationResponseDto>(res);
    }

    public async Task<IOperationResult> GetByEventId(
        PaginationParameters paginationParameters,
        Parameters queryParameters,
        Guid eventId,
        CancellationToken ct
    )
    {
        var entities = await repository.GetByEventId(
            paginationParameters,
            queryParameters,
            eventId,
            ct
        );

        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<EventRegistrationResponseDto>>(res);
    }

    public async Task<IOperationResult> GetByMemberId(
        PaginationParameters paginationParameters,
        Parameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var entities = await repository.GetByMemberId(
            paginationParameters,
            queryParameters,
            memberId,
            ct
        );

        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<EventRegistrationResponseDto>>(res);
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
}
