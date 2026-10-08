using Echo.Domain.Members;
using Riok.Mapperly.Abstractions;

namespace Echo.Application.Members;

[Mapper]
public partial class MemberMapper : IMemberMapper
{
    public MemberResponseDto ToDto(Member entity)
    {
        return new MemberResponseDto
        {
            Id = entity.PersonId,
            Name = entity.Person.Name,
            FirstName = entity.Person.FirstName,
            LastName = entity.Person.LastName,
            OtherNames = entity.Person.OtherNames,
            EmailAddress = entity.Person.EmailAddress,
            PhoneNumber = entity.Person.PhoneNumber,
            DateOfBirth = entity.DateOfBirth,
            JoinedDate = entity.JoinedDate,
            Gender = entity.Gender,
            ResidentialAddress = entity.ResidentialAddress,
            City = entity.City,
            Hometown = entity.Hometown,
            Region = entity.Region,
            GpsAddress = entity.GpsAddress,
            MaritalStatus = entity.MaritalStatus,
            NextOfKin = entity.NextOfKin,
            EmergencyContactName = entity.EmergencyContactName,
            EmergencyContactPhoneNumber = entity.EmergencyContactPhoneNumber,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
        };
    }

    public (Person person, Member member) ToEntity(MemberCreateDto dto)
    {
        var person = new Person
        {
            Kind = PersonKind.Member,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            OtherNames = dto.OtherNames,
            EmailAddress = dto.EmailAddress,
            PhoneNumber = dto.PhoneNumber,
        };

        var member = new Member
        {
            DateOfBirth = dto.DateOfBirth,
            JoinedDate = dto.JoinedDate,
            Gender = dto.Gender,
            ResidentialAddress = dto.ResidentialAddress,
            City = dto.City,
            Hometown = dto.Hometown,
            Region = dto.Region,
            GpsAddress = dto.GpsAddress,
            MaritalStatus = dto.MaritalStatus,
            NextOfKin = dto.NextOfKin,
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhoneNumber = dto.EmergencyContactPhoneNumber,
            Status = dto.Status,
        };

        return (person, member);
    }

    public List<MemberResponseDto> ToListDto(List<Member> entities) =>
        entities.Select(ToDto).ToList();

    public List<MemberSearchResultDto> ToSearchDto(List<Member> entities) =>
        entities
            .Select(m => new MemberSearchResultDto
            {
                Id = m.PersonId,
                Name = m.Person.Name,
                PhoneNumber = m.Person.PhoneNumber,
            })
            .ToList();

    public void Patch(MemberUpdateDto dto, Person person, Member member)
    {
        // Person fields
        if (dto.FirstName != null)
            person.FirstName = dto.FirstName;
        if (dto.LastName != null)
            person.LastName = dto.LastName;
        if (dto.OtherNames != null)
            person.OtherNames = dto.OtherNames;
        if (dto.EmailAddress != null)
            person.EmailAddress = dto.EmailAddress;
        if (dto.PhoneNumber != null)
            person.PhoneNumber = dto.PhoneNumber;

        // Member fields
        if (dto.DateOfBirth.HasValue)
            member.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.JoinedDate.HasValue)
            member.JoinedDate = dto.JoinedDate.Value;
        if (dto.Gender.HasValue)
            member.Gender = dto.Gender.Value;
        if (dto.ResidentialAddress != null)
            member.ResidentialAddress = dto.ResidentialAddress;
        if (dto.City != null)
            member.City = dto.City;
        if (dto.Hometown != null)
            member.Hometown = dto.Hometown;
        if (dto.Region.HasValue)
            member.Region = dto.Region.Value;
        if (dto.GpsAddress != null)
            member.GpsAddress = dto.GpsAddress;
        if (dto.MaritalStatus.HasValue)
            member.MaritalStatus = dto.MaritalStatus.Value;
        if (dto.NextOfKin != null)
            member.NextOfKin = dto.NextOfKin;
        if (dto.EmergencyContactName != null)
            member.EmergencyContactName = dto.EmergencyContactName;
        if (dto.EmergencyContactPhoneNumber != null)
            member.EmergencyContactPhoneNumber = dto.EmergencyContactPhoneNumber;
        if (dto.Status.HasValue)
            member.Status = dto.Status.Value;
    }
}
