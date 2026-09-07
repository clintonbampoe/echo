using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.OrganizationMemberMapping;

[Mapper]
public partial class OrganizationMemberMapper : IOrganizationMemberMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(nameof(OrganizationMember.Member.Name), nameof(OrganizationMemberResponseDto.MemberName))]
    [MapProperty(nameof(OrganizationMember.Organization.Name), nameof(OrganizationMemberResponseDto.OrganizationName))]
    public partial OrganizationMemberResponseDto ToDto(OrganizationMember entity);

    [MapperIgnoreTarget(nameof(OrganizationMember.Congregation))]
    [MapperIgnoreTarget(nameof(OrganizationMember.CongregationId))]
    [MapperIgnoreTarget(nameof(OrganizationMember.Id))]
    [MapperIgnoreTarget(nameof(OrganizationMember.Member))]
    [MapperIgnoreTarget(nameof(OrganizationMember.Organization))]
    [MapperIgnoreTarget(nameof(OrganizationMember.CreatedAt))]
    [MapperIgnoreTarget(nameof(OrganizationMember.DeletedAt))]
    public partial OrganizationMember ToEntity(OrganizationMemberCreateDto dto);

    public void Patch(OrganizationMemberUpdateDto dto, OrganizationMember entity)
    {
        if (dto.Role.HasValue) entity.Role = dto.Role.Value;
        if (dto.JoinedAt.HasValue) entity.JoinedAt = dto.JoinedAt.Value;
    }
}
