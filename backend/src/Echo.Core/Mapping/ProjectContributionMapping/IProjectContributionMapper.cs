using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.ProjectContributionMapping;

public interface IProjectContributionMapper
{
    ProjectContributionResponseDto ToDto(ProjectContribution entity);
    ProjectContribution ToEntity(ProjectContributionCreateDto dto);
    void Patch(ProjectContributionUpdateDto dto, ProjectContribution entity);
}
