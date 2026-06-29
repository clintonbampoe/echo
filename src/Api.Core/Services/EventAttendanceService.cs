using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Dtos.Core;
using Api.Core.Entities;
using Api.Core.Entities.Core;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

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
