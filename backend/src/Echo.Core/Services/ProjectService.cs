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

public class ProjectService(ProjectRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    : PrimaryServiceBase<Project, ProjectResponseDto>(repository, unitOfWork, mapper)
{
    private readonly ProjectRepository _projectRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _projectRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<ProjectListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _projectRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Project not found.");

        return new SuccessResult<ProjectResponseDto>(result);
    }

    public async Task<IOperationResult> GetSummaryAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _projectRepository.GetSummaryAsync(congregationId, ct);
        return new SuccessResult<ProjectSummaryDto>(result);
    }
}
