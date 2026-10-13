using Echo.Domain.Transactions;

namespace Echo.Application.Transactions;

public static class TransactionRules
{
    // A transaction's type must be the type of the category it is filed under.
    public static bool TypeMatches(TransactionType type, TransactionCategory category) =>
        type == category.CategoryType;
}
