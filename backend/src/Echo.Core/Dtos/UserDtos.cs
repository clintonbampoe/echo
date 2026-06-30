using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record UserCreateDto : IPrimaryCreateDto
{
    public required string Name { get; init; }
    public required string UserName { get; init; }
    public required string EmailAddress { get; init; }
    public required string PasswordHash { get; init; }
    public UserRole Role { get; init; }
}

public record UserUpdateDto : IPrimaryUpdateDto
{
    public required string Name { get; init; }
    public required string UserName { get; init; }
    public required string EmailAddress { get; init; }
    public required string PasswordHash { get; init; }
    public UserRole Role { get; init; }
}

public record UserListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string EmailAddress { get; init; }
    public UserRole Role { get; init; }
}

public record UserResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string UserName { get; init; }
    public required string EmailAddress { get; init; }
    public required string PasswordHash { get; init; }
    public UserRole Role { get; init; }
    public DateTime CreatedAt { get; init; }
}
