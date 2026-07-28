using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class AttendanceContextService(
    AttendanceContextRepository repository,
    AppDbContext context,
    IMapper mapper
) : ReferenceServiceBase<AttendanceContext>(repository, context, mapper)
{
    private readonly AttendanceContextRepository _attendanceContextRepository = repository;

    public override async Task<IOperationResult> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _attendanceContextRepository.GetAllAsync(congregationId, ct);
        return new SuccessResult<IEnumerable<AttendanceContextResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        int id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _attendanceContextRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Attendance context not found.");

        return new SuccessResult<AttendanceContextResponseDto>(result);
    }
}
