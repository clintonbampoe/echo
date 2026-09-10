using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.EventMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class EventService(
    EventRepository repository,
    OrganizationRepository organizationRepository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEventMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var entities = await repository.GetPage(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<EventResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(id, congregationId, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<EventResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        EventCreateDto dto,
        CancellationToken ct
    )
    {
        var organizer = await memberRepository.GetById(congregationId, dto.OrganizerId, ct);
        if (organizer is null)
            return new ForeignKeyEntityNotFound(nameof(organizer));

        var organization = await organizationRepository.GetById(
            congregationId,
            dto.OrganizationId,
            ct
        );
        if (organization is null)
            return new ForeignKeyEntityNotFound(nameof(organization));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Organization = organization;
        entity.Organizer = organizer;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

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
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<EventResponseDto>(res);
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

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        var entities = await repository.Search(congregationId, name, ct);
        var res = mapper.ToSearchDto(entities);
        return new SuccessResult<List<EventSearchResultDto>>(res);
    }

    public Task<IOperationResult> GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
