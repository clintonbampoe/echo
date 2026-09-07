using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.AttendanceMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class AttendanceService(
    AttendanceRepository repository,
    IUnitOfWork unitOfWork,
    IAttendanceMapper mapper,
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
        var result = await repository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<AttendanceListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Attendance record not found.");

        return new SuccessResult<AttendanceResponseDto>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, AttendanceCreateDto dto, CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<AttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, Guid id, AttendanceUpdateDto dto,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> GetSummary(
        Guid congregationId,
        int attendanceContextId,
        DateOnly forDate,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetSummaryAsync(
            congregationId,
            attendanceContextId,
            forDate,
            ct
        );
        return new SuccessResult<AttendanceSummaryDto>(result);
    }
}
