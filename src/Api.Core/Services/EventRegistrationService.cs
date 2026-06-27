using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

public class EventRegistrationService(
    EventRegistrationRepository repository,
    AppDbContext context,
    IMapper mapper
) : PrimaryServiceBase<EventRegistration>(repository, context, mapper)
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
        CancellationToken ct = default
    )
    {
        var result = await _eventRegistrationRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Event registration not found.");

        return new SuccessResult<EventRegistrationResponseDto>(result);
    }
}
