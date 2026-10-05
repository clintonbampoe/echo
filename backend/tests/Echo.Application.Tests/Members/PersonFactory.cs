using Echo.Application.Tests.Congregations;
using Echo.Domain.Members;

namespace Echo.Application.Tests.Members;

public static class PersonFactory
{
    public static Person NewMemberPerson()
    {
        return new Person
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            Kind = PersonKind.Member,
            FirstName = "John",
            LastName = "Doe",
            OtherNames = null,
            PhoneNumber = "0244000000",
            EmailAddress = "john.doe@example.com",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static Person NewVisitorPerson()
    {
        return new Person
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            Kind = PersonKind.Visitor,
            FirstName = "Jane",
            LastName = "Smith",
            OtherNames = null,
            PhoneNumber = "0244111111",
            EmailAddress = null,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }
}
