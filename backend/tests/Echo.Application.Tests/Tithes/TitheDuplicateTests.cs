using Echo.Application.Tithes;
using Echo.Domain.Tithes;

namespace Echo.Application.Tests.Tithes;

[Trait("Category", "Unit")]
public class TitheDuplicateTests
{
    [Fact]
    public void DuplicateDetail_NamesTheMemberAndTheMonth_AndSaysToUpdate()
    {
        var detail = TitheService.DuplicateDetail("Kofi Mensah", MonthOfYear.October, 2026);

        Assert.Equal(
            "Kofi Mensah already has a tithe for October 2026. Update that tithe instead.",
            detail
        );
    }

    [Fact]
    public void DuplicateCode_IsStable_SoClientsCanBranchOnIt()
    {
        Assert.Equal("TITHE_DUPLICATE", TitheService.DuplicateCode);
    }
}
