using Echo.Application.Transactions;
using Echo.Domain.Transactions;

namespace Echo.Application.Tests.Transactions;

[Trait("Category", "Unit")]
public class TransactionRulesTests
{
    [Theory]
    [InlineData(TransactionType.Income, TransactionType.Income, true)]
    [InlineData(TransactionType.Expense, TransactionType.Expense, true)]
    [InlineData(TransactionType.Income, TransactionType.Expense, false)]
    [InlineData(TransactionType.Expense, TransactionType.Income, false)]
    public void TypeMatches_OnlyWhenTransactionTypeEqualsCategoryType(
        TransactionType transactionType,
        TransactionType categoryType,
        bool expected
    )
    {
        var category = TestData.Shared.CreateTransactionCategory();
        category.CategoryType = categoryType;

        Assert.Equal(expected, TransactionRules.TypeMatches(transactionType, category));
    }
}
