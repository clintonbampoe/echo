using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

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
