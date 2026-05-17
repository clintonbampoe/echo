using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class OrganizationMember
{
    public int MemberId { get; init; }
    public int OrganizationId { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedDate { get; init; }
}
