using Echo.Domain.Users;
using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Simulation;

[Trait("Category", "Unit")]
public class StaffPlannerTests
{
    [Fact]
    public void StaffedCongregation_HasTenToTwentyUsers_AdminIncluded()
    {
        for (var seed = 1; seed <= 200; seed++)
        {
            var extra = StaffPlanner.ExtraRoles(Rng.ForPhase(seed, "Users"), staffed: true);

            Assert.InRange(extra.Count + 1, 10, 20);
        }
    }

    [Fact]
    public void OtherCongregation_HasOneToFifteenUsers_AdminIncluded()
    {
        for (var seed = 1; seed <= 200; seed++)
        {
            var extra = StaffPlanner.ExtraRoles(Rng.ForPhase(seed, "Users"), staffed: false);

            Assert.InRange(extra.Count + 1, 1, 15);
        }
    }

    [Fact]
    public void StaffedCongregation_HasEveryRole()
    {
        var extra = StaffPlanner.ExtraRoles(Rng.ForPhase(7, "Users"), staffed: true);

        Assert.All(
            new[] { UserRole.Accountant, UserRole.Clerk, UserRole.Member, UserRole.Admin },
            role => Assert.Contains(role, extra)
        );
    }

    [Fact]
    public void LocalParts_AreUniqueAndNumberedPerRole()
    {
        var names = StaffPlanner.LocalParts([
            UserRole.Accountant,
            UserRole.Clerk,
            UserRole.Clerk,
            UserRole.Admin,
            UserRole.Admin,
        ]);

        Assert.Equal(["accountant1", "clerk1", "clerk2", "admin2", "admin3"], names);
    }
}
