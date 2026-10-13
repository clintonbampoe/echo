using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public class SessionLabelService(
    SessionLabelRepository repository,
    IUnitOfWork unitOfWork,
    ISessionLabelMapper mapper,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<SessionLabelService> logger
)
{
    public async Task<IOperationResult> List(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.session_label.list");

        List<SessionLabel> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.session_label.fetch.list"))
        {
            entities = await repository.List(congregationId, ct);
        }

        var data = mapper.ToListDto(entities);
        activity?.SetTag("session_label.count", data.Count);
        SessionLabelLog.Listed(logger, congregationId, data.Count);

        return new SuccessResult<List<SessionLabelResponseDto>>(data);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.session_label.get_by_id"
        );
        activity?.SetTag("session_label.id", id);

        SessionLabel? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session_label.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("session_label.found", false);
            SessionLabelLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        SessionLabelLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<SessionLabelResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        SessionLabelCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.session_label.create"
        );

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        using (instrumentation.ActivitySource.StartActivity("svc.session_label.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("session_label.id", entity.Id);
        SessionLabelLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);
        var location =
            linker.GetPathByName(httpContext, "GetSessionLabelById", new { id = res.Id })
            ?? throw new InvalidOperationException(
                "Route 'GetSessionLabelById' is not registered."
            );
        return new CreatedResult<SessionLabelResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        int id,
        SessionLabelUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.session_label.update"
        );
        activity?.SetTag("session_label.id", id);

        SessionLabel? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session_label.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("session_label.found", false);
            SessionLabelLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.session_label.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        SessionLabelLog.Updated(logger, congregationId, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<SessionLabelResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.session_label.delete"
        );
        activity?.SetTag("session_label.id", id);

        SessionLabel? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session_label.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("session_label.found", false);
            SessionLabelLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.session_label.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        SessionLabelLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.session_label.search"
        );
        activity?.SetTag("session_label.query", name);

        List<SessionLabel> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.session_label.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("session_label.count", res.Count);
        SessionLabelLog.Searched(logger, congregationId, name, res.Count);
        return new SuccessResult<List<SessionLabelSearchResultDto>>(res);
    }
}
