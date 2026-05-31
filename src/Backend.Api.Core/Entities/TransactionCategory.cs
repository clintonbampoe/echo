using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class TransactionCategory : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Name { get; set; } = string.Empty;
    public TransactionType CategoryType { get; set; }
    public DateTime? DeletedAt { get; set; }
}
