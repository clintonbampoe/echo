using Api.Core.Entities.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Entities;

public class User : IPrimaryEntity, ISearchableEntity
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
