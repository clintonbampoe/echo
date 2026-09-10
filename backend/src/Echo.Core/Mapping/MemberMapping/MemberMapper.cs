using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.MemberMapping;

[Mapper]
public partial class MemberMapper : IMemberMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    public partial MemberResponseDto ToDto(Member entity);

    [MapperIgnoreTarget(nameof(Member.Congregation))]
    [MapperIgnoreTarget(nameof(Member.CongregationId))]
    [MapperIgnoreTarget(nameof(Member.Id))]
    [MapperIgnoreTarget(nameof(Member.CreatedAt))]
    [MapperIgnoreTarget(nameof(Member.DeletedAt))]
    public partial Member ToEntity(MemberCreateDto dto);

    public partial List<MemberResponseDto> ToListDto(List<Member> entities);

    public List<MemberSearchResultDto> ToSearchDto(List<Member> entities)
    {
        var res = entities
            .Select(m => new MemberSearchResultDto
            {
                Id = m.Id,
                Name = m.Name,
                PhoneNumber = m.PhoneNumber,
            })
            .ToList();
        return res;
    }

    public void Patch(MemberUpdateDto dto, Member entity)
    {
        if (dto.FirstName != null)
            entity.FirstName = dto.FirstName;
        if (dto.LastName != null)
            entity.LastName = dto.LastName;
        if (dto.OtherNames != null)
            entity.OtherNames = dto.OtherNames;
        if (dto.EmailAddress != null)
            entity.EmailAddress = dto.EmailAddress;
        if (dto.PhoneNumber != null)
            entity.PhoneNumber = dto.PhoneNumber;
        if (dto.DateOfBirth.HasValue)
            entity.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.JoinedDate.HasValue)
            entity.JoinedDate = dto.JoinedDate.Value;
        if (dto.Gender.HasValue)
            entity.Gender = dto.Gender.Value;
        if (dto.ResidentialAddress != null)
            entity.ResidentialAddress = dto.ResidentialAddress;
        if (dto.City != null)
            entity.City = dto.City;
        if (dto.Hometown != null)
            entity.Hometown = dto.Hometown;
        if (dto.Region.HasValue)
            entity.Region = dto.Region.Value;
        if (dto.GpsAddress != null)
            entity.GpsAddress = dto.GpsAddress;
        if (dto.MaritalStatus.HasValue)
            entity.MaritalStatus = dto.MaritalStatus.Value;
        if (dto.NextOfKin != null)
            entity.NextOfKin = dto.NextOfKin;
        if (dto.EmergencyContactName != null)
            entity.EmergencyContactName = dto.EmergencyContactName;
        if (dto.EmergencyContactPhoneNumber != null)
            entity.EmergencyContactPhoneNumber = dto.EmergencyContactPhoneNumber;
        if (dto.MemberActivityStatus.HasValue)
            entity.MemberActivityStatus = dto.MemberActivityStatus.Value;
    }
}
