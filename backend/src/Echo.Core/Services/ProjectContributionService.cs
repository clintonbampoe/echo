using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.ProjectContributionMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class ProjectContributionService(
    ProjectContributionRepository repository,
    ProjectRepository projectRepository,
    IUnitOfWork unitOfWork,
    IProjectContributionMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var entities = await repository.GetPage(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<ProjectContributionResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectContributionResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        ProjectContributionCreateDto dto,
        CancellationToken ct
    )
    {
        var project = await projectRepository.GetById(congregationId, dto.ProjectId, ct);
        if (project is null)
            return new ForeignKeyEntityNotFound(nameof(project));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Project = project;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

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
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectContributionResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        repository.SoftDelete(entity);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }

    public Task<IOperationResult> GetSummary(
        Guid congregationId,
        Guid projectId,
        CancellationToken ct
    )
    {
        throw new NotImplementedException();
    }
}
