using Api.Core.Dtos.Core.Interfaces;

namespace Api.Core.Dtos.Core;

public record OrganizationCreateDto(string Name, string? Description) : IPrimaryCreateDto;

public record OrganizationUpdateDto(string Name, string? Description) : IPrimaryUpdateDto;

public record OrganizationListResponseDto(Guid Id, string Name, string? Description)
    : IPrimaryListResponseDto;

public record OrganizationResponseDto(Guid Id, string Name, string? Description, DateTime CreatedAt)
    : IPrimaryResponseDto;
