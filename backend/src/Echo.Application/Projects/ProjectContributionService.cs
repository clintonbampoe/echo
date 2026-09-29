using Echo.Data;
using Echo.Domain.Projects;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Projects;

public class ProjectContributionService(
    ProjectContributionRepository repository,
    ProjectRepository projectRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IProjectContributionMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        ProjectContributionFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_contribution.list"
        );

        var cursor = encoder.Decode<ProjectContributionCursor>(pagination.Cursor);

        List<ProjectContribution> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.fetch.list"))
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
        activity?.SetTag("project_contribution.count", data.Count);

        var res = new PagedResponse<ProjectContributionResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<ProjectContributionResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_contribution.get_by_id"
        );
        activity?.SetTag("project_contribution.id", id);

        ProjectContribution? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project_contribution.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectContributionResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        ProjectContributionCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_contribution.create"
        );

        Project? project;
        using (
            instrumentation.ActivitySource.StartActivity(
                "svc.project_contribution.validate.project"
            )
        )
        {
            project = await projectRepository.GetById(congregationId, dto.ProjectId, ct);
        }
        if (project is null)
            return new ForeignKeyEntityNotFound(nameof(project));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Project = project;

        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("project_contribution.id", entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<ProjectContributionResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        ProjectContributionUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_contribution.update"
        );
        activity?.SetTag("project_contribution.id", id);

        ProjectContribution? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project_contribution.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectContributionResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_contribution.delete"
        );
        activity?.SetTag("project_contribution.id", id);

        ProjectContribution? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("project_contribution.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    private static ProjectContributionCursor BuildCursor(ProjectContribution last)
    {
        return new ProjectContributionCursor
        {
            DateContributed = last.DateContributed,
            Id = last.Id,
        };
    }
}
