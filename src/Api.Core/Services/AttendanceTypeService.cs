using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

public class AttendanceTypeService(
    AttendanceTypeRepository repository,
    AppDbContext context,
    IMapper mapper
) : ReferenceServiceBase<AttendanceType>(repository, context, mapper)
{
    private readonly AttendanceTypeRepository _attendanceTypeRepository = repository;

    public override async Task<IOperationResult> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _attendanceTypeRepository.GetAllAsync(congregationId, ct);
        return new SuccessResult<IEnumerable<AttendanceTypeResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        var result = await _attendanceTypeRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Attendance type not found.");

        return new SuccessResult<AttendanceTypeResponseDto>(result);
    }
}
