namespace Backend.Api.Core.Entities.Interfaces;
public interface ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Congregation Congregation { get; init; }
}
