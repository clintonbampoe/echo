using Api.Core.Entities.Core.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Entities.Core;

public class TransactionCategory : IReferenceEntity, ISearchableEntity
{
    public int Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public TransactionType CategoryType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
