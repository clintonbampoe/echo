using Echo.Domain.Congregations;

namespace Echo.Domain.Users;

public class User : ISearchable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }

    public string EmailAddress { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? PasswordChangedAt { get; set; }

    /// <summary>When an admin turned the account off; null while it can sign in. The account and its records stay.</summary>
    public DateTime? DisabledAt { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? OtherNames { get; set; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
