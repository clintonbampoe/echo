using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class TransactionCategory
{
    public int CategoryId { get; }
    public Guid UniqueId { get; }
    public string Name { get; }
    public TransactionType CategoryType { get; }
}
