using AutoMapper;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;
using Echo.Shared.Pagination;
using Echo.Shared.Query;

namespace Echo.Core.Services;

public class EventAttendanceService(
    EventAttendanceRepository repository,
    AppDbContext context,
    IMapper mapper
) : PrimaryServiceBase<EventAttendance>(repository, context, mapper)
{
    private readonly EventAttendanceRepository _eventAttendanceRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _eventAttendanceRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<EventAttendanceListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        var result = await _eventAttendanceRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Event attendance record not found.");

        return new SuccessResult<EventAttendanceResponseDto>(result);
    }
}
