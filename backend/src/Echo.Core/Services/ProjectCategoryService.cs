using AutoMapper;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;

namespace Echo.Core.Services;

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
