using AutoMapper;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;
using Echo.Shared.Pagination;
using Echo.Shared.Query;

namespace Echo.Core.Services;

public class ProjectContributionService(
    ProjectContributionRepository repository,
    AppDbContext context,
    IMapper mapper
) : PrimaryServiceBase<ProjectContribution>(repository, context, mapper)
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
        CancellationToken ct = default
    )
    {
        var result = await _projectContributionRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Project contribution not found.");

        return new SuccessResult<ProjectContributionResponseDto>(result);
    }
}
