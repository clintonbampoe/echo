using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class OrganizationMember : ICongregationEntity, ISoftDeletableEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public MemberOrganizationalRole Role { get; set; }
    public DateOnly JoinedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
