using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.ProjectMapping;

public interface IProjectMapper
{
    ProjectResponseDto ToDto(Project entity);
    Project ToEntity(ProjectCreateDto dto);
    void Patch(ProjectUpdateDto dto, Project entity);
}
