namespace Echo.Auth.Dtos;

public record SendVerificationLinkRequest
{
    public string Email { get; init; } = string.Empty;
}
