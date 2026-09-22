using Echo.Shared.HttpResults;
using Echo.Data;

namespace Echo.Application.Projects;

public class ProjectCategoryService(
    ProjectCategoryRepository repository,
    IUnitOfWork unitOfWork,
    IProjectCategoryMapper mapper
)
{
    public async Task<IOperationResult> List(Guid congregationId, CancellationToken ct)
    {
        var entities = await repository.GetAll(congregationId, ct);
        var res = mapper.ToListDto(entities);
        return new SuccessResult<IEnumerable<ProjectCategoryResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(int id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        ProjectCategoryCreateDto dto,
        CancellationToken ct
    )
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<ProjectCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        int id,
        ProjectCategoryUpdateDto dto,
        CancellationToken ct
    )
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<ProjectCategoryResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, int id, CancellationToken ct)
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
        return new SuccessResult<List<ProjectCategorySearchResultDto>>(res);
    }
}
