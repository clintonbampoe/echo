namespace Echo.Core.Dtos;

public record EventAttendanceCreateDto
{
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceUpdateDto
{
    public TimeOnly? CheckInTime { get; init; }
}

public record EventAttendanceListResponseDto
{
    public Guid Id { get; init; }
    public required string MemberName { get; init; }
    public required string EventName { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public Guid EventId { get; init; }
    public required string EventName { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public DateTime CreatedAt { get; init; }
}
