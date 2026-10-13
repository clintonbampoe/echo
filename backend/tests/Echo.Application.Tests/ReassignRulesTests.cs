using Echo.Application.Tests.Transactions;
using Echo.Application.Transactions;
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
        var source = TransactionCategoryFactory.NewEntity();
        var target = TransactionCategoryFactory.NewEntity();

        Assert.Null(ReassignRules.CheckTypes(source, target));
    }

    [Fact]
    public void CheckTypes_DifferentCategoryType_ReturnsAnError()
    {
        var source = TransactionCategoryFactory.NewEntity();
        var target = TransactionCategoryFactory.NewEntity();
        target.CategoryType = TransactionType.Expense;

        Assert.NotNull(ReassignRules.CheckTypes(source, target));
    }
}
