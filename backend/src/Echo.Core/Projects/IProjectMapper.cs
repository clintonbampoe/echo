using Echo.Domain.Projects;

namespace Echo.Core.Projects;

public interface IProjectMapper
{
    ProjectResponseDto ToDto(Project entity);
    Project ToEntity(ProjectCreateDto dto);
    List<ProjectResponseDto> ToListDto(List<Project> entities);

    List<ProjectSearchResultDto> ToSearchDto(List<Project> entities);
    void Patch(ProjectUpdateDto dto, Project entity);
}
