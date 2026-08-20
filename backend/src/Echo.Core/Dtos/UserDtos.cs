using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record UserCreateDto : IPrimaryCreateDto
{
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? OtherNames { get; init; }
    public required string EmailAddress { get; init; }
    public required string Password { get; init; }
    public UserRole Role { get; init; }
}

public record UserUpdateDto : IPrimaryUpdateDto
{
    public required string Name { get; init; }
    public required string EmailAddress { get; init; }
    public required string Password { get; init; }
    public UserRole Role { get; init; }
}

public record UserListResponseDto
    : IPrimaryListResponseDto,
        Application.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string EmailAddress { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime? VerifiedAt { get; init; }
    public UserRole Role { get; init; }
}

public record UserResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string EmailAddress { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public UserRole Role { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UserAuthDto
{
    public Guid Id { get; init; }
    public string EmailAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public DateTime? EmailVerifiedAt { get; init; }
    public UserRole Role { get; init; }
    public Guid CongregationId { get; init; }
}
