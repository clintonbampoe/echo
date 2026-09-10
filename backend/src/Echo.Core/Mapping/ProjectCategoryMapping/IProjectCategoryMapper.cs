using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.ProjectCategoryMapping;

public interface IProjectCategoryMapper
{
    ProjectCategoryResponseDto ToDto(ProjectCategory entity);
    ProjectCategory ToEntity(ProjectCategoryCreateDto dto);
    List<ProjectCategoryResponseDto> ToListDto(List<ProjectCategory> entities);

    List<ProjectCategorySearchResultDto> ToSearchDto(List<ProjectCategory> entities);
    void Patch(ProjectCategoryUpdateDto dto, ProjectCategory entity);
}
