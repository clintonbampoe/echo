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
    IUnitOfWork unitOfWork,
    IEventRegistrationMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetPage(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<EventRegistrationListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Event registration not found.");

        return new SuccessResult<EventRegistrationResponseDto>(result);
    }

    public async Task<IOperationResult> GetByEventId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid eventId,
        CancellationToken ct
    )
    {
        var result = await repository.GetByEventId(
            paginationParameters,
            queryParameters,
            eventId,
            ct
        );

        return new SuccessResult<PagedResponse<EventRegistrationListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetByMemberId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var result = await repository.GetByMemberId(
            paginationParameters,
            queryParameters,
            memberId,
            ct
        );

        return new SuccessResult<PagedResponse<EventRegistrationListResponseDto>>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, EventRegistrationCreateDto dto,
        CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<EventRegistrationResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, Guid id, EventRegistrationUpdateDto dto,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
