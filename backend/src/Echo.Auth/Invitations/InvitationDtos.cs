using System.ComponentModel.DataAnnotations;
using Echo.Domain.Users;

namespace Echo.Auth.Invitations;

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
