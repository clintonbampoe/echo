using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.ProjectCategoryMapping;

public interface IProjectCategoryMapper
{
    ProjectCategoryResponseDto ToDto(ProjectCategory entity);
    ProjectCategory ToEntity(ProjectCategoryCreateDto dto);
    void Patch(ProjectCategoryUpdateDto dto, ProjectCategory entity);
}
