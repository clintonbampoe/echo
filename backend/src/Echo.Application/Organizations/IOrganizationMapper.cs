using Echo.Domain.Organizations;

namespace Echo.Application.Organizations;

public interface IOrganizationMapper
{
    OrganizationResponseDto ToDto(Organization entity);
    Organization ToEntity(OrganizationCreateDto dto);
    List<OrganizationResponseDto> ToListDto(List<Organization> entities);

    List<OrganizationSearchResultDto> ToSearchDto(List<Organization> entities);
    void Patch(OrganizationUpdateDto dto, Organization entity);
}
