using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class EventRegistrationService(
    EventRegistrationRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
)
    : PrimaryServiceBase<EventRegistration, EventRegistrationResponseDto>(
        repository,
        unitOfWork,
        mapper
    )
{
    private readonly EventRegistrationRepository _eventRegistrationRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _eventRegistrationRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<EventRegistrationListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _eventRegistrationRepository.GetByIdAsync(id, congregationId, ct);

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
        var result = await _eventRegistrationRepository.GetByEventId(
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
        var result = await _eventRegistrationRepository.GetByMemberId(
            paginationParameters,
            queryParameters,
            memberId,
            ct
        );

        return new SuccessResult<PagedResponse<EventRegistrationListResponseDto>>(result);
    }
}
