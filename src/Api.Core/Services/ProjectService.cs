using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

public class ProjectService(ProjectRepository repository, AppDbContext context, IMapper mapper)
    : PrimaryServiceBase<Project>(repository, context, mapper)
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
        CancellationToken ct = default
    )
    {
        var result = await _projectRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Project not found.");

        return new SuccessResult<ProjectResponseDto>(result);
    }
}
