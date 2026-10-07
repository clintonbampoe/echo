using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public class AttendanceTypeService(
    AttendanceTypeRepository repository,
    IUnitOfWork unitOfWork,
    IAttendanceTypeMapper mapper,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<AttendanceTypeService> logger
)
{
    public async Task<IOperationResult> List(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.service_type.list");

        List<AttendanceType> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.service_type.fetch.list"))
        {
            entities = await repository.List(congregationId, ct);
        }

        var data = mapper.ToListDto(entities);
        activity?.SetTag("service_type.count", data.Count);
        AttendanceTypeLog.Listed(logger, congregationId, data.Count);

        return new SuccessResult<List<AttendanceTypeResponseDto>>(data);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.service_type.get_by_id"
        );
        activity?.SetTag("service_type.id", id);

        AttendanceType? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.service_type.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("service_type.found", false);
            AttendanceTypeLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        AttendanceTypeLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceTypeResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AttendanceTypeCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.service_type.create"
        );

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        using (instrumentation.ActivitySource.StartActivity("svc.service_type.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("service_type.id", entity.Id);
        AttendanceTypeLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);
        var location =
            linker.GetPathByName(httpContext, "GetAttendanceTypeById", new { id = res.Id })
            ?? throw new InvalidOperationException(
                "Route 'GetAttendanceTypeById' is not registered."
            );
        return new CreatedResult<AttendanceTypeResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        int id,
        AttendanceTypeUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.service_type.update"
        );
        activity?.SetTag("service_type.id", id);

        AttendanceType? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.service_type.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("service_type.found", false);
            AttendanceTypeLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.service_type.persist"))
        {
            mapper.Patch(dto, entity);
            await unitOfWork.CommitAsync(ct);
        }

        AttendanceTypeLog.Updated(logger, congregationId, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceTypeResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.service_type.delete"
        );
        activity?.SetTag("service_type.id", id);

        AttendanceType? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.service_type.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("service_type.found", false);
            AttendanceTypeLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.service_type.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        AttendanceTypeLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.service_type.search"
        );
        activity?.SetTag("service_type.query", name);

        List<AttendanceType> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.service_type.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("service_type.count", res.Count);
        AttendanceTypeLog.Searched(logger, congregationId, name, res.Count);
        return new SuccessResult<List<AttendanceTypeSearchResultDto>>(res);
    }
}
