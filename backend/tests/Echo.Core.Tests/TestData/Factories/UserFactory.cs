using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class UserFactory
{
    public static User NewEntity()
    {
        return new User()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            EmailAddress = Constants.DefaultEmailAddress,
            PasswordHash = "hashed-password",
            Role = UserRole.Admin,
            EmailVerifiedAt = Constants.DefaultDateTime,
            FirstName = Constants.DefaultName,
            LastName = Constants.DefaultName,
            OtherNames = Constants.DefaultName,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<User> NewEntityList()
    {
        var res = new List<User>();

        for (int i = 0; i < 5; i++)
        {
            res.Add(NewEntity());
        }

        return res;
    }

    public static UserCreateDto NewCreateDto()
    {
        return new UserCreateDto()
        {
            EmailAddress = Constants.DefaultEmailAddress,
            Password = "Password",
            Role = UserRole.Admin,
            FirstName = Constants.DefaultName,
            LastName = Constants.DefaultName,
            OtherNames = Constants.DefaultName,
        };
    }

    public static UserUpdateDto NewUpdateDto()
    {
        return new UserUpdateDto()
        {
            EmailAddress = Constants.DefaultEmailAddress,
            Password = "Password",
            Role = UserRole.Admin,
        };
    }

    public static UserUpdateDto NewUpdateDtoWithNullValues()
    {
        return new UserUpdateDto()
        {
            EmailAddress = null,
            Password = null,
            Role = null,
        };
    }
}
