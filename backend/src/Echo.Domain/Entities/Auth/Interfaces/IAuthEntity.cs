using Echo.Domain.Entities.Core.Interfaces;

namespace Echo.Domain.Entities.Auth.Interfaces;

public interface IAuthEntity : ISoftDeletable
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
