using AutoMapper;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;

namespace Echo.Core.Services;

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
