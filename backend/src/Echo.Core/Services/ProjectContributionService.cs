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
    IUnitOfWork unitOfWork,
    IProjectContributionMapper mapper,
    IIdGenerator idGenerator
)

{
    public
        async Task<IOperationResult> GetPage(
            Guid congregationId,
            PaginationParameters paginationParameters,
            QueryParameters? queryParameters,
            CancellationToken ct = default
        )
    {
        var result = await repository.GetPage(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<ProjectContributionListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Project contribution not found.");

        return new SuccessResult<ProjectContributionResponseDto>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, ProjectContributionCreateDto dto,
        CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<ProjectContributionResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, Guid id, ProjectContributionUpdateDto dto,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> GetSummary(
        Guid congregationId,
        Guid projectId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetSummary(
            congregationId,
            projectId,
            ct
        );
        return new SuccessResult<ProjectContributionSummaryDto?>(result);
    }
}
