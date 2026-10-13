using System.ComponentModel.DataAnnotations;
using Echo.Domain.Transactions;

namespace Echo.Application;

public record ReassignCategoryDto
{
    [Range(1, int.MaxValue)]
    public int TargetCategoryId { get; init; }

    public bool DeleteSource { get; init; }
}

public record ReassignCategoryResponseDto
{
    public int Moved { get; init; }
    public bool SourceDeleted { get; init; }
}

public static class ReassignRules
{
    // Each returns an error message, or null when the move is allowed.
    public static string? Check(int sourceId, int targetId) =>
        sourceId == targetId ? "The target category must differ from the source category." : null;

    public static string? CheckTypes(TransactionCategory source, TransactionCategory target) =>
        source.CategoryType == target.CategoryType
            ? null
            : "Transactions can only move to a category of the same type (income or expense).";
}
