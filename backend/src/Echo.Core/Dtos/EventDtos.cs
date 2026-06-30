using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record EventCreateDto : IPrimaryCreateDto
{
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }
    public required string Name { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public int? Capacity { get; init; }
    public string? Description { get; init; }
}

public record EventUpdateDto : IPrimaryUpdateDto
{
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }
    public required string Name { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public int? Capacity { get; init; }
    public string? Description { get; init; }
}

public record EventListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string OrganizationName { get; init; }
    public required string OrganizerName { get; init; }
    public required string Name { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string? Location { get; init; }
}

public record EventResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public Guid OrganizerId { get; init; }
    public required string OrganizerName { get; init; }
    public required string Name { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public int? Capacity { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}
