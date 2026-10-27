using Echo.Application.Organizations;
using Echo.Domain.Members;
using Echo.Domain.Organizations;

namespace Echo.Application.Tests.Organizations;

[Trait("Category", "Unit")]
public class OrganizationSummaryTests
{
    private static readonly DateOnly Today = new(2026, 10, 14);

    private static Organization Org(string name, OrganizationType kind = OrganizationType.Other)
    {
        var org = TestData.Shared.CreateOrganization();
        org.Id = Guid.NewGuid();
        org.Name = name;
        org.Kind = kind;
        return org;
    }

    private static OrganizationMember Join(
        Organization org,
        string name = "Ama",
        MemberRole role = MemberRole.Member,
        DateOnly? joinedAt = null,
        DateOnly? born = null,
        bool deleted = false
    )
    {
        var membership = TestData.Shared.CreateOrganizationMember();
        membership.Id = Guid.NewGuid();
        membership.OrganizationId = org.Id;
        membership.Role = role;
        membership.JoinedAt = joinedAt ?? new DateOnly(2020, 1, 1);
        membership.Member.Person = new Person { Name = name };
        membership.Member.DateOfBirth = born ?? new DateOnly(1990, 1, 1);
        membership.DeletedAt = deleted ? DateTime.UtcNow : null;
        return membership;
    }

    [Fact]
    public void Sizes_CountsEachOrganizationsLiveMembers()
    {
        var choir = Org("Choir");
        var ushers = Org("Ushers");
        var members = new[] { Join(choir), Join(choir), Join(ushers) };

        var res = new[] { choir, ushers }.AsQueryable().Sizes(members.AsQueryable()).ToList();

        Assert.Equal([new OrganizationSize("Choir", 2), new OrganizationSize("Ushers", 1)], res);
    }

    [Fact]
    public void OrganizationStats_From_TotalsAveragesAndNamesTheLargest()
    {
        var sizes = new[]
        {
            new OrganizationSize("Choir", 30),
            new OrganizationSize("Ushers", 10),
            new OrganizationSize("Youth", 20),
        };

        var res = OrganizationStats.From(sizes);

        Assert.Equal((3, 60, 20d, "Choir", 30), (res.Count, res.TotalMembers, res.AverageMembers, res.Largest, res.LargestMembers));
    }

    [Fact]
    public void OrganizationStats_From_WithNoOrganizations_IsAllZeros()
    {
        var res = OrganizationStats.From([]);

        Assert.Equal((0, 0, 0d, (string?)null, 0), (res.Count, res.TotalMembers, res.AverageMembers, res.Largest, res.LargestMembers));
    }

    [Fact]
    public void OrganizationStats_From_BreaksATieForLargestByNameSoTheAnswerIsStable()
    {
        var sizes = new[] { new OrganizationSize("Youth", 5), new OrganizationSize("Choir", 5) };

        Assert.Equal("Choir", OrganizationStats.From(sizes).Largest);
    }

    [Fact]
    public void Leaders_AreTheLiveLeadersOfTheRequestedOrganizationsOnly()
    {
        var choir = Org("Choir");
        var ushers = Org("Ushers");
        var other = Org("Other");
        var members = new[]
        {
            Join(choir, "Kofi", MemberRole.Leader),
            Join(choir, "Ama", MemberRole.Member),
            Join(choir, "Yaw", MemberRole.Leader, deleted: true),
            Join(ushers, "Esi", MemberRole.Leader),
            Join(other, "Nana", MemberRole.Leader),
        };

        var res = members.AsQueryable().Leaders([choir.Id, ushers.Id]).Select(l => l.Name).OrderBy(n => n).ToList();

        Assert.Equal(["Esi", "Kofi"], res);
    }

    [Fact]
    public void FirstLeaders_PicksTheLongestServingLeaderOfEachOrganization_ThenByName()
    {
        var choir = Guid.NewGuid();
        var ushers = Guid.NewGuid();
        var rows = new[]
        {
            new LeaderRow(choir, "Kofi", new DateOnly(2022, 5, 1)),
            new LeaderRow(choir, "Ama", new DateOnly(2019, 3, 1)),
            new LeaderRow(ushers, "Yaw", new DateOnly(2021, 1, 1)),
            new LeaderRow(ushers, "Esi", new DateOnly(2021, 1, 1)),
        };

        var res = OrganizationService.FirstLeaders(rows);

        Assert.Equal(new Dictionary<Guid, string> { [choir] = "Ama", [ushers] = "Esi" }, res);
    }

    [Fact]
    public void AverageAges_AreInWholeYearsOnTheDay_PerOrganization()
    {
        var choir = Org("Choir");
        var members = new[]
        {
            Join(choir, born: new DateOnly(1996, 10, 14)),
            Join(choir, born: new DateOnly(1986, 10, 15)),
            Join(choir, born: new DateOnly(1986, 10, 15), deleted: true),
        };

        var res = members
            .Where(m => m.DeletedAt == null)
            .AsQueryable()
            .AverageAges([choir.Id], Today)
            .Single();

        Assert.Equal((choir.Id, 34.5), (res.Id, res.Years));
    }

    [Fact]
    public void Filter_ByKind_KeepsOnlyThatKind()
    {
        var choir = Org("Choir", OrganizationType.Music);
        var ushers = Org("Ushers", OrganizationType.Service);

        var res = new[] { choir, ushers }
            .AsQueryable()
            .Filter(new OrganizationFilters { Kind = OrganizationType.Music, From = null, To = null })
            .ToList();

        Assert.Equal([choir], res);
    }

    [Fact]
    public void Paginate_Cursor_KeepsOrganizationsAfterItByNameThenId()
    {
        Organization Named(string name, string id)
        {
            var org = Org(name);
            org.Id = Guid.Parse(id);
            return org;
        }

        var cursor = new OrganizationCursor { Name = "Kofi", Id = Guid.Parse("00000000-0000-0000-0000-000000000005") };
        var before = Named("Kofi", "00000000-0000-0000-0000-000000000004");
        var after = Named("Kofi", "00000000-0000-0000-0000-000000000006");
        var earlier = Named("Ama", "00000000-0000-0000-0000-000000000009");
        var later = Named("Yaw", "00000000-0000-0000-0000-000000000001");

        var res = new[] { before, after, earlier, later }.AsQueryable().Paginate(cursor, 10).ToList();

        Assert.Equal([after, later], res);
    }
}
