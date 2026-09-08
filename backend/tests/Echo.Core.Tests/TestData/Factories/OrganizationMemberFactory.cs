using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class OrganizationMemberFactory
{
    public static OrganizationMember NewEntity()
    {
        return new OrganizationMember()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            MemberId = Constants.DefaultGuid,
            Member = MemberFactory.NewEntity(),
            OrganizationId = Constants.DefaultGuid,
            Organization = OrganizationFactory.NewEntity(),
            Role = default,
            JoinedAt = Constants.DefaultDateOnly,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static OrganizationMemberCreateDto NewCreateDto()
    {
        return new OrganizationMemberCreateDto()
        {
            MemberId = Constants.DefaultGuid,
            OrganizationId = Constants.DefaultGuid,
            Role = default,
            JoinedAt = Constants.DefaultDateOnly,
        };
    }

    public static OrganizationMemberUpdateDto NewUpdateDto()
    {
        return new OrganizationMemberUpdateDto()
        {
            Role = MemberOrganizationalRole.Member,
            JoinedAt = Constants.DefaultDateOnly,
        };
    }

    public static OrganizationMemberUpdateDto NewUpdateDtoWithNullValues()
    {
        return new OrganizationMemberUpdateDto()
        {
            Role = null,
            JoinedAt = null
        };
    }
}
