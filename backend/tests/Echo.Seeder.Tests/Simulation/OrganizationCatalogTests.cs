using Echo.Domain.Organizations;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Simulation;

[Trait("Category", "Unit")]
public class OrganizationCatalogTests
{
    [Theory]
    [InlineData("Choir", OrganizationType.Music)]
    [InlineData("Ushering Team", OrganizationType.Service)]
    [InlineData("Women's Fellowship", OrganizationType.Fellowship)]
    [InlineData("Sunday School Teachers", OrganizationType.Teaching)]
    [InlineData("Evangelism Team", OrganizationType.Outreach)]
    [InlineData("Prayer Team", OrganizationType.Prayer)]
    [InlineData("Elders' Council", OrganizationType.Leadership)]
    public void TypeOf_NamesWhatTheMinistryIsFor(string name, OrganizationType expected)
    {
        Assert.Equal(expected, OrganizationCatalog.TypeOf(name));
    }

    [Fact]
    public void TypeOf_AnUnknownMinistry_IsOther()
    {
        Assert.Equal(OrganizationType.Other, OrganizationCatalog.TypeOf("Something New"));
    }

    // A new ministry added to the catalog should be given a type; only the drama team is "other".
    [Fact]
    public void EveryMinistryInTheCatalogHasAType_ExceptTheDramaTeam()
    {
        var untyped = OrganizationCatalog
            .All.Where(k => OrganizationCatalog.TypeOf(k.Name) == OrganizationType.Other)
            .Select(k => k.Name);

        Assert.Equal(["Drama Team"], untyped);
    }

    [Fact]
    public void MeetingDayOf_GivesEachTypeItsUsualDay_AndOtherNone()
    {
        Assert.Equal(DayOfWeek.Thursday, OrganizationCatalog.MeetingDayOf(OrganizationType.Music));
        Assert.Equal(DayOfWeek.Friday, OrganizationCatalog.MeetingDayOf(OrganizationType.Prayer));
        Assert.Null(OrganizationCatalog.MeetingDayOf(OrganizationType.Other));
    }
}
