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

public class EventService(EventRepository repository, AppDbContext context, IMapper mapper)
    : PrimaryServiceBase<Event>(repository, context, mapper)
{
    private readonly EventRepository _eventRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _eventRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<EventListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _eventRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Event not found.");

        return new SuccessResult<EventResponseDto>(result);
    }

    public async Task<IOperationResult> GetSummaryAsync(Guid congregationId, CancellationToken ct = default)
    {
        var result = await _eventRepository.GetSummaryAsync(congregationId, ct);
        return new SuccessResult<EventSummaryDto>(result);
    }
}
