using Echo.Domain.Organizations;

namespace Echo.Core.Organizations;

public interface IOrganizationMemberMapper
{
    OrganizationMemberResponseDto ToDto(OrganizationMember entity);
    OrganizationMember ToEntity(OrganizationMemberCreateDto dto);
    List<OrganizationMemberResponseDto> ToListDto(List<OrganizationMember> entities);

    void Patch(OrganizationMemberUpdateDto dto, OrganizationMember entity);
}
