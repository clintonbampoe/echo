using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.ProjectCategoryMapping;

[Mapper]
public partial class ProjectCategoryMapper : IProjectCategoryMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.CreatedAt))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    public partial ProjectCategoryResponseDto ToDto(ProjectCategory entity);

    [MapperIgnoreTarget(nameof(ProjectCategory.Congregation))]
    [MapperIgnoreTarget(nameof(ProjectCategory.CongregationId))]
    [MapperIgnoreTarget(nameof(ProjectCategory.Id))]
    [MapperIgnoreTarget(nameof(ProjectCategory.CreatedAt))]
    [MapperIgnoreTarget(nameof(ProjectCategory.DeletedAt))]
    public partial ProjectCategory ToEntity(ProjectCategoryCreateDto dto);

    public void Patch(ProjectCategoryUpdateDto dto, ProjectCategory entity)
    {
        if (dto.Name != null) entity.Name = dto.Name;
    }
}
