namespace Api.Core.Dtos.Auth;

public record CreateEmailVerificationTokenDto
{
    public DateTime ExpiresAt { get; init; }
}

public record UpdateEmailVerificationTokenDto
{
    public DateTime ExpiresAt { get; init; }
    public bool IsUsed { get; init; }
}

public record GetEmailVerificationTokenDto
{
    public Guid Id { get; init; }
    public string ForUser { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public bool IsUsed { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record GetEmailVerificationTokenListDto
{
    public Guid Id { get; init; }
    public string ForUser { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public bool IsUsed { get; init; }
}
