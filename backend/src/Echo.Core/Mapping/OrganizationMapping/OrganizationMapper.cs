using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.OrganizationMapping;

[Mapper]
public partial class OrganizationMapper : IOrganizationMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    public partial OrganizationResponseDto ToDto(Organization entity);

    [MapperIgnoreTarget(nameof(Organization.Congregation))]
    [MapperIgnoreTarget(nameof(Organization.CongregationId))]
    [MapperIgnoreTarget(nameof(Organization.Id))]
    [MapperIgnoreTarget(nameof(Organization.CreatedAt))]
    [MapperIgnoreTarget(nameof(Organization.DeletedAt))]
    public partial Organization ToEntity(OrganizationCreateDto dto);

    public partial List<OrganizationResponseDto> ToListDto(List<Organization> entities);

    public List<OrganizationSearchResultDto> ToSearchDto(List<Organization> entities)
    {
        var res = entities
            .Select(e => new OrganizationSearchResultDto() { Id = e.Id, Name = e.Name })
            .ToList();
        return res;
    }

    public void Patch(OrganizationUpdateDto dto, Organization entity)
    {
        if (dto.Name != null)
            entity.Name = dto.Name;
        if (dto.Description != null)
            entity.Description = dto.Description;
    }
}
