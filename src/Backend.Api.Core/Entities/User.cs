using Backend.Api.Core.Entities.Interfaces;

public class User : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity
{
    public string Name { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
