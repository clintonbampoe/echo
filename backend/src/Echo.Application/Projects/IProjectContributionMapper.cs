using Echo.Domain.Projects;

namespace Echo.Application.Projects;

public interface IProjectContributionMapper
{
    ProjectContributionResponseDto ToDto(ProjectContribution entity);
    ProjectContribution ToEntity(ProjectContributionCreateDto dto);
    List<ProjectContributionResponseDto> ToListDto(List<ProjectContribution> entities);

    void Patch(ProjectContributionUpdateDto dto, ProjectContribution entity);
}
