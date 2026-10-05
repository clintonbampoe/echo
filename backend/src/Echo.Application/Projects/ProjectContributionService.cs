using Echo.Data;
using Echo.Domain.Projects;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Projects;

public class ProjectContributionService(
    ProjectContributionRepository repository,
    ProjectRepository projectRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IProjectContributionMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    ILogger<ProjectContributionService> logger
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
        ProjectContributionLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<ProjectContributionResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<ProjectContributionResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
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
            ProjectContributionLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        ProjectContributionLog.Found(logger, congregationId, id);
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
        {
            ProjectContributionLog.CreateProjectNotFound(logger, congregationId, dto.ProjectId);
            return new ForeignKeyEntityNotFound(nameof(project));
        }
        ProjectContributionLog.CreateProjectFound(logger, congregationId, dto.ProjectId);

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
        ProjectContributionLog.Created(logger, congregationId, entity.Id);

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
            ProjectContributionLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        // ProjectContributionUpdateDto only updates amount, date, paymentMethod, description
        // ProjectId is not updatable so no FK validation needed here

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        ProjectContributionLog.Updated(logger, congregationId, id);

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
            ProjectContributionLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.project_contribution.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        ProjectContributionLog.Deleted(logger, congregationId, id);

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

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        ProjectContributionFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.project_contribution.summary"
        );

        var totalTask = repository.SumContributed(congregationId, filters, ct);
        var countTask = repository.Count(congregationId, filters, ct);
        var averageTask = repository.Average(congregationId, filters, ct);
        var paymentMethodTask = repository.MostUsedPaymentMethod(congregationId, filters, ct);

        await Task.WhenAll(totalTask, countTask, averageTask, paymentMethodTask);

        var res = new ProjectContributionSummaryDto
        {
            TotalContributed = totalTask.Result,
            TotalContributions = countTask.Result,
            AverageAmount = averageTask.Result,
            MostUsedPaymentMethod = paymentMethodTask.Result?.ToString(),
        };

        activity?.SetTag("project_contribution.summary.total", res.TotalContributed);
        ProjectContributionLog.Summarized(logger, congregationId, res.TotalContributed);

        return new SuccessResult<ProjectContributionSummaryDto>(res);
    }
}
