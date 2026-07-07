using Echo.Core.Dtos;

namespace Echo.Auth.Dtos;

public record RegisterCongregationRequest
{
    public required CongregationCreateDto CongregationDto { get; init; }
    public required UserCreateDto UserDto { get; init; }
}
