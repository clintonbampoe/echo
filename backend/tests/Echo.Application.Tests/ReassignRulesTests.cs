using AutoFixture;
using Echo.Domain.Transactions;

namespace Echo.Application.Tests;

[Trait("Category", "Unit")]
public class ReassignRulesTests
{
    [Fact]
    public void Check_TargetIsTheSource_ReturnsAnError()
    {
        Assert.NotNull(ReassignRules.Check(3, 3));
    }

    [Fact]
    public void Check_TargetIsDifferent_ReturnsNull()
    {
        Assert.Null(ReassignRules.Check(3, 4));
    }

    [Fact]
    public void CheckTypes_SameCategoryType_ReturnsNull()
    {
        Assert.Null(
            ReassignRules.CheckTypes(
                NewCategory(TransactionType.Income),
                NewCategory(TransactionType.Income)
            )
        );
    }

    [Fact]
    public void CheckTypes_DifferentCategoryType_ReturnsAnError()
    {
        Assert.NotNull(
            ReassignRules.CheckTypes(
                NewCategory(TransactionType.Income),
                NewCategory(TransactionType.Expense)
            )
        );
    }

    private static TransactionCategory NewCategory(TransactionType type) =>
        TestData.Build(
            TestData.Shared,
            () =>
                TestData
                    .Shared.Build<TransactionCategory>()
                    .With(c => c.CategoryType, type)
                    .Create()
                    .Live()
        );
}
