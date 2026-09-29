using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Members;
using Echo.Domain.Projects;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Projects;

public class ProjectService(
    ProjectRepository repository,
    MemberRepository memberRepository,
    ProjectCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IProjectMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        ProjectFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.project.list");

        var cursor = encoder.Decode<ProjectCursor>(pagination.Cursor);

        List<Project> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.project.fetch.list"))
        {
            entities = await repository.List(
                congregationId,
                filters,
                cursor,
                pagination.PageSize + 1,
                ct
            );
        }

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("project.count", data.Count);

        var res = new PagedResponse<ProjectResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<ProjectResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.project.get_by_id");
        activity?.SetTag("project.id", id);

        Project? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        ProjectCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.project.create");

        Member? projectManager;
        using (instrumentation.ActivitySource.StartActivity("svc.project.validate.manager"))
        {
            projectManager = await memberRepository.GetById(congregationId, dto.ManagerId, ct);
        }
        if (projectManager is null)
            return new ForeignKeyEntityNotFound(nameof(projectManager));

        ProjectCategory? projectCategory;
        using (instrumentation.ActivitySource.StartActivity("svc.project.validate.category"))
        {
            projectCategory = await categoryRepository.GetById(congregationId, dto.CategoryId, ct);
        }
        if (projectCategory is null)
            return new ForeignKeyEntityNotFound(nameof(projectCategory));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Category = projectCategory;
        entity.Manager = projectManager;

        using (instrumentation.ActivitySource.StartActivity("svc.project.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("project.id", entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<ProjectResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        ProjectUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.project.update");
        activity?.SetTag("project.id", id);

        Project? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.project.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.project.delete");
        activity?.SetTag("project.id", id);

        Project? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.project.persist"))
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
        using var activity = instrumentation.ActivitySource.StartActivity("svc.project.search");
        activity?.SetTag("project.query", name);

        List<Project> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.project.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("project.count", res.Count);
        return new SuccessResult<List<ProjectSearchResultDto>>(res);
    }

    private static ProjectCursor BuildCursor(Project last)
    {
        return new ProjectCursor { StartDate = last.StartDate, Id = last.Id };
    }
}
