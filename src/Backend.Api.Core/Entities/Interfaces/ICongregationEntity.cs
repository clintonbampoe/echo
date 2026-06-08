namespace Backend.Api.Core.Entities.Interfaces;

public abstract class ICongregationEntity
{
    public virtual Guid Id { get; set; }
    public virtual Guid CongregationId { get; set; }
    public virtual Congregation Congregation { get; set; } = null!;
    public virtual DateTime CreatedAt { get; set; }
}
