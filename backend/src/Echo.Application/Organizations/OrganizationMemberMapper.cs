using Echo.Domain.Organizations;
using Riok.Mapperly.Abstractions;

namespace Echo.Application.Organizations;

[Mapper]
public partial class OrganizationMemberMapper : IOrganizationMemberMapper
{
    public OrganizationMemberResponseDto ToDto(OrganizationMember entity) =>
        new OrganizationMemberResponseDto
        {
            Id = entity.Id,
            MemberId = entity.MemberId,
            MemberName = entity.Member.Person.Name,
            OrganizationId = entity.OrganizationId,
            OrganizationName = entity.Organization.Name,
            Role = entity.Role,
            JoinedAt = entity.JoinedAt,
            CreatedAt = entity.CreatedAt,
        };

    [MapperIgnoreTarget(nameof(OrganizationMember.Congregation))]
    [MapperIgnoreTarget(nameof(OrganizationMember.CongregationId))]
    [MapperIgnoreTarget(nameof(OrganizationMember.Id))]
    [MapperIgnoreTarget(nameof(OrganizationMember.Member))]
    [MapperIgnoreTarget(nameof(OrganizationMember.Organization))]
    [MapperIgnoreTarget(nameof(OrganizationMember.CreatedAt))]
    [MapperIgnoreTarget(nameof(OrganizationMember.DeletedAt))]
    public partial OrganizationMember ToEntity(OrganizationMemberCreateDto dto);

    public partial List<OrganizationMemberResponseDto> ToListDto(List<OrganizationMember> entities);

    public void Patch(OrganizationMemberUpdateDto dto, OrganizationMember entity)
    {
        if (dto.Role.HasValue)
            entity.Role = dto.Role.Value;
        if (dto.JoinedAt.HasValue)
            entity.JoinedAt = dto.JoinedAt.Value;
    }
}
