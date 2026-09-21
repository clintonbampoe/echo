using Echo.Domain.Congregations;

namespace Echo.Domain.Users;

public class User : ISearchable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public string EmailAddress { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? OtherNames { get; set; } = string.Empty;
    public string Name { get; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
