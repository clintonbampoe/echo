using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record OrganizationCreateDto(string Name, string? Description)
    : IPrimaryCreateDto<Organization>;

public record OrganizationUpdateDto(string Name, string? Description)
    : IPrimaryUpdateDto<Organization>;

public record OrganizationListResponseDto(Guid Id, string Name, string? Description)
    : IPrimaryListResponseDto<Organization>;

public record OrganizationResponseDto(Guid Id, string Name, string? Description, DateTime CreatedAt)
    : IPrimaryResponseDto<Organization>;
