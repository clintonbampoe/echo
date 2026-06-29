using Api.Core.Dtos.Core.Interfaces;

namespace Api.Core.Dtos.Core;

public record EventCreateDto(
    Guid OrganizationId,
    Guid OrganizerId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Location,
    int? Capacity,
    string? Description
) : IPrimaryCreateDto;

public record EventUpdateDto(
    Guid OrganizationId,
    Guid OrganizerId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Location,
    int? Capacity,
    string? Description
) : IPrimaryUpdateDto;

public record EventListResponseDto(
    Guid Id,
    string OrganizationName,
    string OrganizerName,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Location
) : IPrimaryListResponseDto;

public record EventResponseDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid OrganizerId,
    string OrganizerName,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Location,
    int? Capacity,
    string? Description,
    DateTime CreatedAt
) : IPrimaryResponseDto;
