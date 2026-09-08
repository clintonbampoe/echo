using Echo.Application.HttpResults;
using Echo.Core.Dtos;
using Echo.Core.Mapping.AttendanceTypeMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class AttendanceTypeService(
    AttendanceTypeRepository repository,
    IUnitOfWork unitOfWork,
    IAttendanceTypeMapper mapper
)
{
    public async Task<IOperationResult> GetAll(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetAllAsync(congregationId, ct);
        return new SuccessResult<IEnumerable<AttendanceTypeResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        int id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Attendance type not found.");

        return new SuccessResult<AttendanceTypeResponseDto>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, AttendanceTypeCreateDto dto, CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<AttendanceTypeResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, int id, AttendanceTypeUpdateDto dto,
        CancellationToken ct)
    {
        var entity = await repository.GetEntityById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceTypeResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        var entity = await repository.GetEntityById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        await repository.SoftDelete(entity, ct);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }
}
