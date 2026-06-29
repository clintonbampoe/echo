namespace Api.Core.Entities.Core.Interfaces;

public interface IPrimaryEntity : ICongregationEntity
{
    Guid Id { get; set; }
}
