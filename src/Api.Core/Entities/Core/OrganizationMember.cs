using Api.Core.Entities.Core.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Entities.Core;

public class OrganizationMember : IPrimaryEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public MemberOrganizationalRole Role { get; set; }
    public DateOnly JoinedAt { get; set; }
}
