namespace Echo.Domain.Entities.Core.Interfaces;

public interface ISoftDeletable
{
    public DateTime? DeletedAt { get; set; }
}
