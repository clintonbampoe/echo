using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Transaction
{
    public int TransactionId { get; }
    public Guid UniqueId { get; }
    public TransactionType TransactionType { get; }
    public decimal amount { get; }
    public int CategoryId { get; }
    public string Description { get; }
}
