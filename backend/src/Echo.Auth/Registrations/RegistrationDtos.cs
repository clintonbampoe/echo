using System.ComponentModel.DataAnnotations;
using Echo.Application.Congregations;
using Echo.Application.Users;

namespace Echo.Auth.Registrations;

public record RegisterCongregationRequest
{
    [Required]
    public required CongregationCreateDto CongregationDto { get; init; }

    [Required]
    public required UserCreateDto UserDto { get; init; }
}

public record RegisterMemberRequest
{
    [Required, StringLength(512, MinimumLength = 1)]
    public required string Token { get; init; }

    [Required]
    public required UserCreateDto UserInfo { get; init; }
}
