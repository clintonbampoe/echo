using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record OrganizationCreateDto : IPrimaryCreateDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public record OrganizationUpdateDto : IPrimaryUpdateDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public record OrganizationListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public record OrganizationResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}
