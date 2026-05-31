using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class AttendanceRecord : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public Guid MemberId { get; init; }
    public DateOnly ForDate { get; set; }
    public ChurchServiceType ChurchServiceType { get; set; }
    public AttendeeType AttendeeType { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
