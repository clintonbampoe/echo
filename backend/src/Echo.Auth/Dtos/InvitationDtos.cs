using Echo.Core.Dtos;
using Echo.Domain.Enums;

namespace Echo.Auth.Dtos;

public record InviteRequest
{
    public required UserRole AllowedRole { get; init; }
    public int? ExpiryDays { get; init; }
}

public record InviteResponseDto
{
    public required string Token { get; init; }
    public required UserRole AllowedRole { get; init; }
    public required DateTime ExpiresAt { get; init; }
}

public record RegisterMemberRequest
{
    public required string Token { get; init; }
    public required UserCreateDto UserInfo { get; init; }
}
