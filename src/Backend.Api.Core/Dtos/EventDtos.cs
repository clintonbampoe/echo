using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

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
) : IPrimaryCreateDto<Event>;

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
) : IPrimaryUpdateDto<Event>;

public record EventListResponseDto(
    Guid Id,
    string OrganizationName,
    string OrganizerName,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Location
) : IPrimaryListResponseDto<Event>;

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
) : IPrimaryResponseDto<Event>;
