using Echo.Domain.Projects;

namespace Echo.Core.Projects;

public interface IProjectCategoryMapper
{
    ProjectCategoryResponseDto ToDto(ProjectCategory entity);
    ProjectCategory ToEntity(ProjectCategoryCreateDto dto);
    List<ProjectCategoryResponseDto> ToListDto(List<ProjectCategory> entities);

    List<ProjectCategorySearchResultDto> ToSearchDto(List<ProjectCategory> entities);
    void Patch(ProjectCategoryUpdateDto dto, ProjectCategory entity);
}
