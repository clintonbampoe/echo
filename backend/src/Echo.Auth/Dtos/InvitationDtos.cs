using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos;
using Echo.Domain.Enums;

namespace Echo.Auth.Dtos;

public record InviteRequest
{
    public required UserRole AllowedRole { get; init; }

    [Range(1, 365)]
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
    [Required, StringLength(512, MinimumLength = 1)]
    public required string Token { get; init; }

    [Required]
    public required UserCreateDto UserInfo { get; init; }
}
