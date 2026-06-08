using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class OrganizationMember : ICongregationEntity, ISoftDeletableEntity
{
    public Guid MemberId { get; init; }
    public Member Member { get; init; } = null!;
    public Guid OrganizationId { get; init; }
    public Organization Organization { get; init; } = null!;
    public MemberOrganizationalRole Role { get; set; }
    public DateOnly JoinedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
