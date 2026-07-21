namespace Echo.Auth.Dtos;

public record ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordRequest
{
    public string Email { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}
