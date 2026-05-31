using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class OrganizationMember : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid OrganizationId { get; init; }
    public MemberOrganizationalRole Role { get; init; }
    public DateOnly JoinedDate { get; init; }
    public DateTime? DeletedAt{ get; set; }
}
