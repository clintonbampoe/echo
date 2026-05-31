using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class OrganizationMember : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid OrganizationId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public MemberOrganizationalRole Role { get; set; }
    public DateOnly JoinedDate { get; set; }
    public DateTime? DeletedAt { get; set; }
}
