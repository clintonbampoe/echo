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
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _eventAttendanceRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Event attendance record not found.");

        return new SuccessResult<EventAttendanceResponseDto>(result);
    }
}
