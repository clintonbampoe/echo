using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.ProjectMapping;

[Mapper]
public partial class ProjectMapper : IProjectMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(nameof(Project.Category.Name), nameof(ProjectResponseDto.CategoryName))]
    [MapProperty(nameof(Project.Manager.Name), nameof(ProjectResponseDto.ManagerName))]
    public partial ProjectResponseDto ToDto(Project entity);

    [MapperIgnoreTarget(nameof(Project.Congregation))]
    [MapperIgnoreTarget(nameof(Project.CongregationId))]
    [MapperIgnoreTarget(nameof(Project.Id))]
    [MapperIgnoreTarget(nameof(Project.Category))]
    [MapperIgnoreTarget(nameof(Project.Manager))]
    [MapperIgnoreTarget(nameof(Project.CreatedAt))]
    [MapperIgnoreTarget(nameof(Project.DeletedAt))]
    public partial Project ToEntity(ProjectCreateDto dto);

    public partial List<ProjectResponseDto> ToListDto(List<Project> entities);

    public List<ProjectSearchResultDto> ToSearchDto(List<Project> entities)
    {
        var res = entities
            .Select(e => new ProjectSearchResultDto() { Id = e.Id, Name = e.Name })
            .ToList();
        return res;
    }

    public void Patch(ProjectUpdateDto dto, Project entity)
    {
        if (dto.CategoryId.HasValue)
            entity.CategoryId = dto.CategoryId.Value;
        if (dto.ManagerId.HasValue)
            entity.ManagerId = dto.ManagerId.Value;
        if (dto.Name != null)
            entity.Name = dto.Name;
        if (dto.TargetAmount.HasValue)
            entity.TargetAmount = dto.TargetAmount.Value;
        if (dto.Status.HasValue)
            entity.Status = dto.Status.Value;
        if (dto.StartDate.HasValue)
            entity.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue)
            entity.EndDate = dto.EndDate.Value;
        if (dto.Description != null)
            entity.Description = dto.Description;
    }
}
