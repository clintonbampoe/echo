using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class OrganizationMember : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Congregation Congregation { get; init; } = null!;
    public Guid MemberId { get; init; }
    public Member Member { get; init; } = null!;
    public Guid OrganizationId { get; init; }
    public Organization Organization { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public MemberOrganizationalRole Role { get; set; }
    public DateOnly JoinedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
