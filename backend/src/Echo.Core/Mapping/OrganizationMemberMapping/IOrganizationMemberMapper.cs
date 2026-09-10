using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.OrganizationMemberMapping;

public interface IOrganizationMemberMapper
{
    OrganizationMemberResponseDto ToDto(OrganizationMember entity);
    OrganizationMember ToEntity(OrganizationMemberCreateDto dto);
    List<OrganizationMemberResponseDto> ToListDto(List<OrganizationMember> entities);

    void Patch(OrganizationMemberUpdateDto dto, OrganizationMember entity);
}
