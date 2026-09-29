using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.HttpResults;

namespace Echo.Application.Attendances;

public class AttendanceContextService(
    AttendanceContextRepository repository,
    IUnitOfWork unitOfWork,
    IAttendanceContextMapper mapper,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_context.list"
        );

        List<AttendanceContext> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_context.fetch.all"))
        {
            entities = await repository.GetAll(congregationId, ct);
        }

        var res = mapper.ToListDto(entities);
        activity?.SetTag("attendance_context.count", res.Count);

        return new SuccessResult<List<AttendanceContextResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(int id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_context.get_by_id"
        );
        activity?.SetTag("attendance_context.id", id);

        AttendanceContext? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_context.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance_context.found", false);
            return new ForeignKeyEntityNotFound(nameof(entity.AttendanceType));
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceContextResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AttendanceContextCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_context.create"
        );

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        using (instrumentation.ActivitySource.StartActivity("svc.attendance_context.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("attendance_context.id", entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<AttendanceContextResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        int id,
        AttendanceContextUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_context.update"
        );
        activity?.SetTag("attendance_context.id", id);

        AttendanceContext? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_context.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance_context.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.attendance_context.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceContextResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_context.delete"
        );
        activity?.SetTag("attendance_context.id", id);

        AttendanceContext? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_context.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance_context.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.attendance_context.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance_context.search"
        );
        activity?.SetTag("attendance_context.query", name);

        List<AttendanceContext> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance_context.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("attendance_context.count", res.Count);

        return new SuccessResult<List<AttendanceContextSearchResultDto>>(res);
    }
}
