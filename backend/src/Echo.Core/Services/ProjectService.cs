using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.ProjectMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class ProjectService(
    ProjectRepository repository,
    MemberRepository memberRepository,
    ProjectCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IProjectMapper mapper,
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
        return new SuccessResult<List<ProjectResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        ProjectCreateDto dto,
        CancellationToken ct
    )
    {
        var projectManager = await memberRepository.GetById(congregationId, dto.ManagerId, ct);
        if (projectManager is null)
            return new ForeignKeyEntityNotFound(nameof(projectManager));

        var projectCategory = await categoryRepository.GetById(congregationId, dto.CategoryId, ct);
        if (projectCategory is null)
            return new ForeignKeyEntityNotFound(nameof(projectCategory));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Category = projectCategory;
        entity.Manager = projectManager;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<ProjectResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        ProjectUpdateDto dto,
        CancellationToken ct
    )
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectResponseDto>(res);
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

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        var entities = await repository.Search(congregationId, name, ct);
        var res = mapper.ToSearchDto(entities);
        return new SuccessResult<List<ProjectSearchResultDto>>(res);
    }

    public Task<IOperationResult> GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
