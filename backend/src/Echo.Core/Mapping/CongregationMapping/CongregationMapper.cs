using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.CongregationMapping;

[Mapper]
public partial class CongregationMapper : ICongregationMapper
{
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    public partial CongregationResponseDto ToDto(Congregation entity);

    [MapperIgnoreTarget(nameof(Congregation.Id))]
    [MapperIgnoreTarget(nameof(Congregation.CreatedAt))]
    [MapperIgnoreTarget(nameof(Congregation.DeletedAt))]
    public partial Congregation ToEntity(CongregationCreateDto dto);

    public partial List<CongregationResponseDto> ToListDto(List<Congregation> entities);

    public List<CongregationSearchResultDto> ToSearchDto(List<Congregation> entities)
    {
        var res = entities
            .Select(e => new CongregationSearchResultDto() { Id = e.Id, Name = e.Name })
            .ToList();
        return res;
    }

    public void Patch(CongregationUpdateDto dto, Congregation entity)
    {
        if (dto.Name != null)
            entity.Name = dto.Name;
        if (dto.PhoneNumber != null)
            entity.PhoneNumber = dto.PhoneNumber;
        if (dto.EmailAddress != null)
            entity.EmailAddress = dto.EmailAddress;
        if (dto.PostalAddress != null)
            entity.PostalAddress = dto.PostalAddress;
        if (dto.WebsiteUrl != null)
            entity.WebsiteUrl = dto.WebsiteUrl;
        if (dto.Region.HasValue)
            entity.Region = dto.Region.Value;
        if (dto.OrgType.HasValue)
            entity.OrgType = dto.OrgType.Value;
        if (dto.City != null)
            entity.City = dto.City;
        if (dto.Town != null)
            entity.Town = dto.Town;
        if (dto.GpsAddress != null)
            entity.GpsAddress = dto.GpsAddress;
    }
}
