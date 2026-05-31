using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class AttendanceRecord : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public DateOnly ForDate { get; init; }
    public ChurchServiceType ChurchServiceType { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
