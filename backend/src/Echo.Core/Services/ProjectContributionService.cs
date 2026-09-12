using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Services.Encoders;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.ProjectContributionMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class ProjectContributionService(
    ProjectContributionRepository repository,
    ProjectRepository projectRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IProjectContributionMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        ProjectContributionFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var cursor = encoder.Decode<ProjectContributionCursor>(pagination.Cursor);
        var entities = await repository.List(
            congregationId,
            filters,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<ProjectContributionResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<ProjectContributionResponseDto>>(res);
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

    private ProjectContributionCursor BuildCursor(ProjectContribution last)
    {
        return new ProjectContributionCursor
        {
            DateContributed = last.DateContributed,
            Id = last.Id,
        };
    }
}
