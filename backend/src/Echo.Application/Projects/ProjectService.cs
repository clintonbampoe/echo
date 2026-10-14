using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Members;
using Echo.Domain.Projects;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Echo.Shared.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Projects;

public class ProjectService(
    ProjectRepository repository,
    MemberRepository memberRepository,
    ProjectCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IProjectMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    DateWindows dateWindows,
    ILogger<ProjectService> logger
)
{
    internal static DateWindow? ResolveWindow(ProjectFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisYear(filters.From, filters.To, hasParent: filters.ManagerId is not null);

    public async Task<IOperationResult> List(
        Guid congregationId,
        ProjectFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

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

        var counts = await repository.GetRaisedAmounts(
            congregationId,
            entities.Select(x => x.Id).ToList(),
            ct
        );
        var data = entities
            .Select(x => mapper.ToDto(x) with { RaisedAmount = counts.GetValueOrDefault(x.Id) })
            .ToList();
        activity?.SetTag("project.count", data.Count);
        ProjectLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<ProjectResponseDto>(hasMore, nextCursor, data, window);
        return new SuccessResult<PagedResponse<ProjectResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
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
            ProjectLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        ProjectLog.Found(logger, congregationId, id);
        var res = await WithCount(entity, congregationId, ct);
        return new SuccessResult<ProjectResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        ProjectCreateDto dto,
        HttpContext httpContext,
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
        {
            ProjectLog.CreateManagerNotFound(logger, congregationId, dto.ManagerId);
            return new ForeignKeyEntityNotFound(nameof(projectManager));
        }
        ProjectLog.CreateManagerFound(logger, congregationId, dto.ManagerId);

        ProjectCategory? projectCategory;
        using (instrumentation.ActivitySource.StartActivity("svc.project.validate.category"))
        {
            projectCategory = await categoryRepository.GetById(congregationId, dto.CategoryId, ct);
        }
        if (projectCategory is null)
        {
            ProjectLog.CreateCategoryNotFound(logger, congregationId, dto.CategoryId);
            return new ForeignKeyEntityNotFound(nameof(projectCategory));
        }
        ProjectLog.CreateCategoryFound(logger, congregationId, dto.CategoryId);

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
        ProjectLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);

        var location =
            linker.GetPathByName(httpContext, "GetProjectById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetProjectById' is not registered.");
        return new CreatedResult<ProjectResponseDto>(location, res);
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
            ProjectLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        // Validate the NEW manager, not the existing one.
        if (dto.ManagerId.HasValue && dto.ManagerId.Value != entity.ManagerId)
        {
            Member? manager;
            using (instrumentation.ActivitySource.StartActivity("svc.project.validate.manager"))
            {
                manager = await memberRepository.GetById(congregationId, dto.ManagerId.Value, ct);
            }

            if (manager is null)
            {
                ProjectLog.UpdateManagerNotFound(logger, congregationId, dto.ManagerId.Value);
                return new ForeignKeyEntityNotFound(nameof(manager));
            }
            ProjectLog.UpdateManagerFound(logger, congregationId, dto.ManagerId.Value);
        }

        // Validate the NEW category, not the existing one.
        if (dto.CategoryId.HasValue && dto.CategoryId.Value != entity.CategoryId)
        {
            ProjectCategory? category;
            using (instrumentation.ActivitySource.StartActivity("svc.project.validate.category"))
            {
                category = await categoryRepository.GetById(
                    congregationId,
                    dto.CategoryId.Value,
                    ct
                );
            }

            if (category is null)
            {
                ProjectLog.UpdateCategoryNotFound(logger, congregationId, dto.CategoryId.Value);
                return new ForeignKeyEntityNotFound(nameof(category));
            }
            ProjectLog.UpdateCategoryFound(logger, congregationId, dto.CategoryId.Value);
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.project.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        ProjectLog.Updated(logger, congregationId, id);

        var res = await WithCount(entity, congregationId, ct);
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
            ProjectLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.project.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        ProjectLog.Deleted(logger, congregationId, id);

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
        ProjectLog.Searched(logger, congregationId, name, res.Count);
        return new SuccessResult<List<ProjectSearchResultDto>>(res);
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        ProjectFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.project.summary");

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new ProjectSummaryDto
        {
            Window = window,
            TotalProjects = stats.Count,
            TotalRaised = stats.Raised,
            TotalTarget = stats.Target,
            AtRiskCount = stats.AtRisk,
        };

        activity?.SetTag("project.summary.total", res.TotalProjects);
        ProjectLog.Summarized(logger, congregationId, res.TotalProjects);

        return new SuccessResult<ProjectSummaryDto>(res);
    }

    private static ProjectCursor BuildCursor(Project last)
    {
        return new ProjectCursor { StartDate = last.StartDate, Id = last.Id };
    }

    private async Task<ProjectResponseDto> WithCount(
        Project entity,
        Guid congregationId,
        CancellationToken ct
    )
    {
        var counts = await repository.GetRaisedAmounts(congregationId, [entity.Id], ct);
        return mapper.ToDto(entity) with { RaisedAmount = counts.GetValueOrDefault(entity.Id) };
    }
}
