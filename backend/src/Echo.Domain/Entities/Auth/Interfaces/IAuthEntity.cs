namespace Echo.Domain.Entities.Auth.Interfaces;

public interface IAuthEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
