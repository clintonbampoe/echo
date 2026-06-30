using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record EventAttendanceCreateDto : IPrimaryCreateDto
{
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceUpdateDto : IPrimaryUpdateDto
{
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string MemberName { get; init; }
    public required string EventName { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public Guid EventId { get; init; }
    public required string EventName { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public DateTime CreatedAt { get; init; }
}
