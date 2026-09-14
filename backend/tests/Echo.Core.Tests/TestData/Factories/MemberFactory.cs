using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class MemberFactory
{
    public static Member NewEntity()
    {
        return new Member()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            FirstName = Constants.DefaultName,
            LastName = Constants.DefaultName,
            OtherNames = Constants.DefaultName,
            EmailAddress = Constants.DefaultEmailAddress,
            PhoneNumber = Constants.DefaultPhoneNumber,
            DateOfBirth = Constants.DefaultDateOnly,
            JoinedDate = Constants.DefaultDateOnly,
            Gender = default,
            ResidentialAddress = Constants.DefaultPostalAddress,
            City = Constants.DefaultCity,
            Hometown = Constants.DefaultTown,
            Region = default,
            GpsAddress = Constants.DefaultGpsAddress,
            MaritalStatus = default,
            NextOfKin = "Kin-01",
            EmergencyContactName = Constants.DefaultName,
            EmergencyContactPhoneNumber = Constants.DefaultPhoneNumber,
            Status = default,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<Member> NewEntityList()
    {
        var res = new List<Member>();
        for (int i = 0; i < 5; i++)
        {
            res.Add(NewEntity());
        }

        return res;
    }

    public static MemberCreateDto NewCreateDto()
    {
        return new MemberCreateDto()
        {
            FirstName = Constants.DefaultName,
            LastName = Constants.DefaultName,
            OtherNames = Constants.DefaultName,
            EmailAddress = Constants.DefaultEmailAddress,
            PhoneNumber = Constants.DefaultPhoneNumber,
            DateOfBirth = Constants.DefaultDateOnly,
            JoinedDate = Constants.DefaultDateOnly,
            Gender = Gender.Female,
            ResidentialAddress = Constants.DefaultPostalAddress,
            City = Constants.DefaultCity,
            Hometown = Constants.DefaultTown,
            Region = Region.Ahafo,
            GpsAddress = Constants.DefaultGpsAddress,
            MaritalStatus = MaritalStatus.Married,
            NextOfKin = "Kin-01",
            EmergencyContactName = Constants.DefaultName,
            EmergencyContactPhoneNumber = Constants.DefaultPhoneNumber,
            Status = MemberStatus.Active,
        };
    }

    public static MemberUpdateDto NewUpdateDto()
    {
        return new MemberUpdateDto()
        {
            FirstName = Constants.DefaultName,
            LastName = Constants.DefaultName,
            OtherNames = Constants.DefaultName,
            EmailAddress = Constants.DefaultEmailAddress,
            PhoneNumber = Constants.DefaultPhoneNumber,
            DateOfBirth = Constants.DefaultDateOnly,
            JoinedDate = Constants.DefaultDateOnly,
            Gender = Gender.Female,
            ResidentialAddress = Constants.DefaultPostalAddress,
            City = Constants.DefaultCity,
            Hometown = Constants.DefaultTown,
            Region = Region.Ahafo,
            GpsAddress = Constants.DefaultGpsAddress,
            MaritalStatus = MaritalStatus.Married,
            NextOfKin = "Kin-01",
            EmergencyContactName = Constants.DefaultName,
            EmergencyContactPhoneNumber = Constants.DefaultPhoneNumber,
            Status = MemberStatus.Active,
        };
    }

    public static MemberUpdateDto NewUpdateDtoWithNullValues()
    {
        return new MemberUpdateDto()
        {
            FirstName = null,
            LastName = null,
            OtherNames = null,
            EmailAddress = null,
            PhoneNumber = null,
            DateOfBirth = null,
            JoinedDate = null,
            Gender = null,
            ResidentialAddress = null,
            City = null,
            Hometown = null,
            Region = null,
            GpsAddress = null,
            MaritalStatus = null,
            NextOfKin = null,
            EmergencyContactName = null,
            EmergencyContactPhoneNumber = null,
            Status = null,
        };
    }
}
