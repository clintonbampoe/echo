using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class AttendanceRecord : ICongregationEntity, ISoftDeletableEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateOnly ForDate { get; set; }
    public ChurchServiceType ChurchServiceType { get; set; }
    public AttendeeType AttendeeType { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
