using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

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
