using Backend.Api.Core.Entities.Interfaces;

public class User : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string EmailAddress { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? DeletedAt { get; set; }

}
