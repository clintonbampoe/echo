using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

public class ProjectCategoryService(
    ProjectCategoryRepository repository,
    AppDbContext context,
    IMapper mapper
) : ReferenceServiceBase<ProjectCategory>(repository, context, mapper)
{
    private readonly ProjectCategoryRepository _projectCategoryRepository = repository;

    public override async Task<IOperationResult> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _projectCategoryRepository.GetAllAsync(congregationId, ct);
        return new SuccessResult<IEnumerable<ProjectCategoryResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        var result = await _projectCategoryRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Project category not found.");

        return new SuccessResult<ProjectCategoryResponseDto>(result);
    }
}
