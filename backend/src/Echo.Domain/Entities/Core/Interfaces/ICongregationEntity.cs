namespace Echo.Domain.Entities.Core.Interfaces;

public interface ICongregationEntity
{
    Guid CongregationId { get; set; }
    Congregation Congregation { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? DeletedAt { get; set; }
}
