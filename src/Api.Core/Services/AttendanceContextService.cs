using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Dtos.Core;
using Api.Core.Entities;
using Api.Core.Entities.Core;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

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
        CancellationToken ct = default
    )
    {
        var result = await _attendanceContextRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Attendance context not found.");

        return new SuccessResult<AttendanceContextResponseDto>(result);
    }
}
