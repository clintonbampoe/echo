namespace Backend.Api.Core.Entities.Interfaces;

public interface IPrimaryEntity : ICongregationEntity
{
    Guid Id { get; set; }
}
