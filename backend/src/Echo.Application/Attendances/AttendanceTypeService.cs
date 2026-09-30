using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.HttpResults;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public class AttendanceTypeService(
    AttendanceTypeRepository repository,
    IUnitOfWork unitOfWork,
    IAttendanceTypeMapper mapper,
    ApplicationInstrumentation instrumentation,
    ILogger<AttendanceTypeService> logger
)
{
    public async Task<IOperationResult> List(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_type.list"
        );

        List<AttendanceType> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_type.fetch.all"))
        {
            entities = await repository.GetAll(congregationId, ct);
        }

        var res = mapper.ToListDto(entities);
        activity?.SetTag("attendance_type.count", res.Count);

        return new SuccessResult<List<AttendanceTypeResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(int id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_type.get_by_id"
        );
        activity?.SetTag("attendance_type.id", id);

        AttendanceType? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_type.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance_type.found", false);
            AttendanceTypeLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceTypeResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AttendanceTypeCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_type.create"
        );

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        using (instrumentation.ActivitySource.StartActivity("svc.attendance_type.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("attendance_type.id", entity.Id);
        AttendanceTypeLog.Created(logger, entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<AttendanceTypeResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        int id,
        AttendanceTypeUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_type.update"
        );
        activity?.SetTag("attendance_type.id", id);

        AttendanceType? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_type.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance_type.found", false);
            AttendanceTypeLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.attendance_type.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        AttendanceTypeLog.Updated(logger, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceTypeResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_type.delete"
        );
        activity?.SetTag("attendance_type.id", id);

        AttendanceType? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_type.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance_type.found", false);
            AttendanceTypeLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.attendance_type.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        AttendanceTypeLog.Deleted(logger, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_type.search"
        );
        activity?.SetTag("attendance_type.query", name);

        List<AttendanceType> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_type.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("attendance_type.count", res.Count);

        return new SuccessResult<List<AttendanceTypeSearchResultDto>>(res);
    }
}
