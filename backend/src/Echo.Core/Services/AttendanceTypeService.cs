using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class AttendanceTypeService(
    AttendanceTypeRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
) : ReferenceServiceBase<AttendanceType, AttendanceTypeResponseDto>(repository, unitOfWork, mapper)
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
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _attendanceTypeRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Attendance type not found.");

        return new SuccessResult<AttendanceTypeResponseDto>(result);
    }
}
