using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

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
