using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class ProjectContributionService(
    ProjectContributionRepository repository,
    IUnitOfWork context,
    IMapper mapper
)
    : PrimaryServiceBase<ProjectContribution, ProjectContributionResponseDto>(
        repository,
        context,
        mapper
    )
{
    private readonly ProjectContributionRepository _projectContributionRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _projectContributionRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<ProjectContributionListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _projectContributionRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Project contribution not found.");

        return new SuccessResult<ProjectContributionResponseDto>(result);
    }

    public async Task<IOperationResult> GetSummaryAsync(
        Guid congregationId,
        Guid projectId,
        CancellationToken ct = default
    )
    {
        var result = await _projectContributionRepository.GetSummaryAsync(
            congregationId,
            projectId,
            ct
        );
        return new SuccessResult<ProjectContributionSummaryDto?>(result);
    }
}
