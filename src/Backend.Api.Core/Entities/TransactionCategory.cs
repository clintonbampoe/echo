using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class TransactionCategory
{
    public int CategoryId { get; init; }
    public Guid UniqueId { get; init; }
    public string Name { get; init; } = string.Empty;
    public TransactionType CategoryType { get; init; }
}
