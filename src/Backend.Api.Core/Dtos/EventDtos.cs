using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record EventCreateDto : ICreateDto<Event>
{
    public Guid CongregationId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public int? Capacity { get; init; }
    public string? Description { get; init; }
}

public record EventResponseDto : IResponseDto<Event>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid OrganizationId { get; init; }
    public string Organization { get; init; } = string.Empty;
    public Guid OrganizerId { get; init; }
    public string Organizer { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public int? Capacity { get; init; }
    public string? Description { get; init; }
}

public record EventListResponseDto : IListResponseDto<Event>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Organization { get; init; } = string.Empty;
    public string Organizer { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string? Location { get; init; }
}

public record EventUpdateDto : IUpdateDto<Event>
{
    public Guid CongregationId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public int? Capacity { get; init; }
    public string? Description { get; init; }
}

public record EventDeleteDto : ISoftDeleteDto<Event>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
