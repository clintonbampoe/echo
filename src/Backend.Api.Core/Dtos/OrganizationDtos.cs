using Backend.Api.Core.Dtos.Interfaces;

namespace Backend.Api.Core.Dtos;

public record OrganizationCreateDto(string Name, string? Description) : IPrimaryCreateDto;

public record OrganizationUpdateDto(string Name, string? Description) : IPrimaryUpdateDto;

public record OrganizationListResponseDto(Guid Id, string Name, string? Description)
    : IPrimaryListResponseDto;

public record OrganizationResponseDto(Guid Id, string Name, string? Description, DateTime CreatedAt)
    : IPrimaryResponseDto;
