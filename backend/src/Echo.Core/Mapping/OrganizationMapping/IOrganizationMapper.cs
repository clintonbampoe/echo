using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.OrganizationMapping;

public interface IOrganizationMapper
{
    OrganizationResponseDto ToDto(Organization entity);
    Organization ToEntity(OrganizationCreateDto dto);
    List<OrganizationResponseDto> ToListDto(List<Organization> entities);

    List<OrganizationSearchResultDto> ToSearchDto(List<Organization> entities);
    void Patch(OrganizationUpdateDto dto, Organization entity);
}
