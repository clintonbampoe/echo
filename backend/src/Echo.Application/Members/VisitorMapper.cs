using Echo.Domain.Members;

namespace Echo.Application.Members;

public class VisitorMapper : IVisitorMapper
{
    public VisitorResponseDto ToDto(Visitor entity)
    {
        return new VisitorResponseDto
        {
            Id = entity.PersonId,
            Name = entity.Person.Name,
            FirstName = entity.Person.FirstName,
            LastName = entity.Person.LastName,
            PhoneNumber = entity.Person.PhoneNumber,
            EmailAddress = entity.Person.EmailAddress,
            Notes = entity.Notes,
            ConvertedToMemberPersonId = entity.ConvertedToMemberPersonId,
            ConvertedToMemberName = entity.ConvertedToMember?.Person.Name,
            ConvertedAt = entity.ConvertedAt,
            CreatedAt = entity.CreatedAt,
        };
    }

    public (Person person, Visitor visitor) ToEntity(VisitorCreateDto dto)
    {
        var person = new Person
        {
            Kind = PersonKind.Visitor,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber ?? string.Empty,
            EmailAddress = dto.EmailAddress,
        };

        var visitor = new Visitor { Notes = dto.Notes };

        return (person, visitor);
    }

    public List<VisitorResponseDto> ToListDto(List<Visitor> entities) =>
        entities.Select(ToDto).ToList();

    public List<VisitorSearchResultDto> ToSearchDto(List<Visitor> entities) =>
        entities
            .Select(v => new VisitorSearchResultDto
            {
                Id = v.PersonId,
                Name = v.Person.Name,
                PhoneNumber = v.Person.PhoneNumber,
            })
            .ToList();

    public void Patch(VisitorUpdateDto dto, Person person, Visitor visitor)
    {
        if (dto.FirstName != null)
            person.FirstName = dto.FirstName;
        if (dto.LastName != null)
            person.LastName = dto.LastName;
        if (dto.PhoneNumber != null)
            person.PhoneNumber = dto.PhoneNumber;
        if (dto.EmailAddress != null)
            person.EmailAddress = dto.EmailAddress;
        if (dto.Notes != null)
            visitor.Notes = dto.Notes;
    }

    public Member ToMemberEntity(MemberCreateDto dto)
    {
        return new Member
        {
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
            DateOfBirth = dto.DateOfBirth,
        };
    }
}
