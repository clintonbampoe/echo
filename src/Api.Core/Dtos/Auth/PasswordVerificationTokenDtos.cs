namespace Api.Core.Dtos.Auth;

public record CreatePasswordVerificationTokenDto
{
    public DateTime ExpiresAt { get; init; }
}

public record UpdatePasswordVerificationTokenDto
{
    public DateTime ExpiresAt { get; init; }
    public bool IsUsed { get; init; }
}

public record GetPasswordVerificationTokenDto
{
    public Guid Id { get; init; }
    public string ForUser { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public bool IsUsed { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record GetPasswordVerificationTokenListDto
{
    public Guid Id { get; init; }
    public string ForUser { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public bool IsUsed { get; init; }
}
