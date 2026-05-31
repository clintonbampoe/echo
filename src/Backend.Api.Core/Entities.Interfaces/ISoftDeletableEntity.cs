namespace Backend.Api.Core.Entities.Interfaces;

public interface ISoftDeletableEntity
{
    public DateTime? DeletedAt { get; set; }
}
