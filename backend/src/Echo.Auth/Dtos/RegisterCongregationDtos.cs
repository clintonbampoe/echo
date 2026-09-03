using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos;

namespace Echo.Auth.Dtos;

public record RegisterCongregationRequest
{
    [Required]
    public required CongregationCreateDto CongregationDto { get; init; }

    [Required]
    public required UserCreateDto UserDto { get; init; }
}
