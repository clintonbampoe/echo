using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record InvitationTokenCreateDto : IPrimaryCreateDto
{
    public Guid CreatedByUserId { get; init; }
    public UserRole AllowedRole { get; init; }
    public required string Token { get; init; }
    public DateTime ExpiryDate { get; init; }
    public bool IsRevoked { get; init; }
}

public record InvitationTokenListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string CreatedByUserEmail { get; init; }
    public UserRole AllowedRole { get; init; }
    public required string Token { get; init; }
    public bool IsRevoked { get; init; }
}

public record InvitationTokenResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public required string Congregation { get; init; }
    public required string CreatedByUserEmail { get; init; }
    public UserRole AllowedRole { get; init; }
    public required string Token { get; init; }
    public DateTime ExpiryDate { get; init; }
    public bool IsRevoked { get; init; }
    public DateTime CreatedAt { get; init; }
}
