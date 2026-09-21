using Echo.Domain.Congregations;
using Echo.Domain.Members;

namespace Echo.Domain.Organizations;

public class OrganizationMember : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public MemberRole Role { get; set; }
    public DateOnly JoinedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
