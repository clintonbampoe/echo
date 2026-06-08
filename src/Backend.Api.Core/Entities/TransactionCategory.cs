using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class TransactionCategory : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity
{
    public string Name { get; set; } = string.Empty;
    public TransactionType CategoryType { get; set; }
    public DateTime? DeletedAt { get; set; }
}
