public class User
{
    public Guid UserId { get; init; }
    public Guid CongregationId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string EmailAddress { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

}
