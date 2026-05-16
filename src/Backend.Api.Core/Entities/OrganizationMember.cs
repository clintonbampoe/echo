using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class OrganizationMember
{
    public int MemberId { get; }
    public int OrganizationId { get; }
    public MemberOrganizationalRole Role { get; }
    public DateOnly JoinedDate { get; }
}
