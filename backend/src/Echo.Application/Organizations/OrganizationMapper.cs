using Echo.Domain.Organizations;
using Riok.Mapperly.Abstractions;

namespace Echo.Application.Organizations;

[Mapper]
public partial class OrganizationMapper : IOrganizationMapper
{
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    // Filled by the service from batched queries.
    [MapperIgnoreTarget(nameof(OrganizationResponseDto.MemberCount))]
    [MapperIgnoreTarget(nameof(OrganizationResponseDto.LeaderName))]
    [MapperIgnoreTarget(nameof(OrganizationResponseDto.AverageAge))]
    public partial OrganizationResponseDto ToDto(Organization entity);

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
        if (dto.Kind.HasValue)
            entity.Kind = dto.Kind.Value;
        if (dto.MeetingDay.HasValue)
            entity.MeetingDay = dto.MeetingDay.Value;
    }
}
