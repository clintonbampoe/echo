using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record EventCreateDto : IPrimaryCreateDto
{
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }

    [StringLength(255)]
    public string? Location { get; init; }

    [Range(1, 100_000)]
    public int? Capacity { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record EventUpdateDto : IPrimaryUpdateDto
{
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }

    [StringLength(255)]
    public string? Location { get; init; }

    [Range(1, 100_000)]
    public int? Capacity { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record EventListResponseDto : IPrimaryListResponseDto, Application.Dtos.Interfaces.IPrimaryListResponseDto
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

public record EventSummaryDto
{
    public required int TotalEvents { get; init; }
    public required int UpcomingEvents { get; init; }
    public required int PastEvents { get; init; }
    public required int TotalRegistrations { get; init; }
}
