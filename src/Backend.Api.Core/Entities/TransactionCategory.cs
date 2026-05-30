using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class TransactionCategory : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public TransactionType CategoryType { get; init; }
}
