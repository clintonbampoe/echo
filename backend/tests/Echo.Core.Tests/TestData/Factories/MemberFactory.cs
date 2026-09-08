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
            MemberActivityStatus = default,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
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
            MemberActivityStatus = MemberActivityStatus.Active,
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
            MemberActivityStatus = MemberActivityStatus.Active,
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
            MemberActivityStatus = null
        };
    }
}
