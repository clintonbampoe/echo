using Echo.Application.HttpResults;
using Echo.Core.Dtos;
using Echo.Core.Mapping.AttendanceContextMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class AttendanceContextService(
    AttendanceContextRepository repository,
    IUnitOfWork unitOfWork,
    IAttendanceContextMapper mapper
)
{
    public async Task<IOperationResult> GetAll(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetAll(congregationId, ct);
        return new SuccessResult<IEnumerable<AttendanceContextResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        int id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Attendance context not found.");

        return new SuccessResult<AttendanceContextResponseDto>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, AttendanceContextCreateDto dto,
        CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<AttendanceContextResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, int id, AttendanceContextUpdateDto dto,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
