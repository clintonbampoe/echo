using Echo.Core.Organizations;
using Echo.Core.Tests.Members;
using Echo.Domain.Organizations;

namespace Echo.Core.Tests.Organizations;

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
            Role = MemberRole.Member,
            JoinedAt = Constants.DefaultDateOnly,
        };
    }

    public static OrganizationMemberUpdateDto NewUpdateDtoWithNullValues()
    {
        return new OrganizationMemberUpdateDto() { Role = null, JoinedAt = null };
    }
}
