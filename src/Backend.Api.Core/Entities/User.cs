using Backend.Api.Core.Entities.Interfaces;

public class User : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string EmailAddress { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }

}
