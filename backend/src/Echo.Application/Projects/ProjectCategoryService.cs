using Echo.Data;
using Echo.Domain.Projects;
using Echo.Shared.HttpResults;

namespace Echo.Application.Projects;

public class ProjectCategoryService(
    ProjectCategoryRepository repository,
    IUnitOfWork unitOfWork,
    IProjectCategoryMapper mapper,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_category.list"
        );

        List<ProjectCategory> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.project_category.fetch.all"))
        {
            entities = await repository.GetAll(congregationId, ct);
        }

        var res = mapper.ToListDto(entities);
        activity?.SetTag("project_category.count", res.Count);

        return new SuccessResult<IEnumerable<ProjectCategoryResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(int id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_category.get_by_id"
        );
        activity?.SetTag("project_category.id", id);

        ProjectCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project_category.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        ProjectCategoryCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_category.create"
        );

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        using (instrumentation.ActivitySource.StartActivity("svc.project_category.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("project_category.id", entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<ProjectCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        int id,
        ProjectCategoryUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_category.update"
        );
        activity?.SetTag("project_category.id", id);

        ProjectCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project_category.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.project_category.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_category.delete"
        );
        activity?.SetTag("project_category.id", id);

        ProjectCategory? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project_category.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project_category.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.project_category.persist"))
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
            "svc.project_category.search"
        );
        activity?.SetTag("project_category.query", name);

        List<ProjectCategory> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.project_category.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("project_category.count", res.Count);
        return new SuccessResult<List<ProjectCategorySearchResultDto>>(res);
    }
}
