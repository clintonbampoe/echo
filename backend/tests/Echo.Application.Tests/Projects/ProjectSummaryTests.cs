using Echo.Application.Projects;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Shared.Utilities;

namespace Echo.Application.Tests.Projects;

[Trait("Category", "Unit")]
public class ProjectSummaryTests
{
    private static readonly Guid Congregation = Guid.NewGuid();

    private static Project Proj(
        decimal target,
        ProjectStatus status = ProjectStatus.OnTrack,
        int categoryId = 1
    )
    {
        var project = TestData.Shared.CreateProject();
        project.Id = Guid.NewGuid();
        project.CongregationId = Congregation;
        project.TargetAmount = target;
        project.Status = status;
        project.CategoryId = categoryId;
        return project;
    }

    private static ProjectContribution Gift(
        Project project,
        decimal amount,
        DateOnly? date = null,
        PaymentMethod method = PaymentMethod.Cash
    )
    {
        var gift = TestData.Shared.CreateProjectContribution();
        gift.Id = Guid.NewGuid();
        gift.ProjectId = project.Id;
        gift.Amount = amount;
        gift.DateContributed = date ?? new DateOnly(2026, 6, 1);
        gift.PaymentMethod = method;
        return gift;
    }

    [Fact]
    public void SummarizeProjects_CountsTargetsAndAtRisk_AndSumsOnlyGiftsToThoseProjects()
    {
        var a = Proj(1000m, ProjectStatus.AtRisk);
        var b = Proj(500m);
        var other = Proj(9000m);
        var gifts = new[] { Gift(a, 100m), Gift(a, 50m), Gift(b, 25m), Gift(other, 999m) };

        var res = new[] { a, b }.AsQueryable().SummarizeProjects(gifts.AsQueryable()).Single();

        Assert.Equal((2, 175m, 1500m, 1), (res.Count, res.Raised, res.Target, res.AtRisk));
    }

    [Fact]
    public void SummarizeProjects_WithNoGifts_RaisedIsZero()
    {
        var res = new[] { Proj(500m) }
            .AsQueryable()
            .SummarizeProjects(Array.Empty<ProjectContribution>().AsQueryable())
            .Single();

        Assert.Equal(0m, res.Raised);
    }

    [Fact]
    public void ContributionTotalsPerProject_SumsAndCountsLiveGiftsOfTheRequestedProjectsOnly()
    {
        var a = Proj(1m);
        var b = Proj(1m);
        var c = Proj(1m);
        var deleted = Gift(a, 1000m);
        deleted.DeletedAt = DateTime.UtcNow;
        var gifts = new[] { Gift(a, 100m), Gift(a, 50.5m), deleted, Gift(b, 20m), Gift(c, 999m) };

        var res = gifts
            .AsQueryable()
            .ContributionTotalsPerProject([a.Id, b.Id])
            .ToDictionary(x => x.Id);

        Assert.Equal(new ProjectContributionTotal(a.Id, 150.5m, 2), res[a.Id]);
        Assert.Equal(new ProjectContributionTotal(b.Id, 20m, 1), res[b.Id]);
        Assert.False(res.ContainsKey(c.Id));
    }

    [Fact]
    public void TotalsByCategory_SumsEachCategorysProjectsAndGiftsAndSortsByRaisedDescending()
    {
        var building1 = Proj(1000m, categoryId: 1);
        var building2 = Proj(500m, categoryId: 1);
        var mission = Proj(2000m, categoryId: 2);
        var gifts = new[] { Gift(building1, 100m), Gift(building2, 50m), Gift(mission, 700m) };
        var categories = new[]
        {
            new ProjectCategory { Id = 1, Name = "Building" },
            new ProjectCategory { Id = 2, Name = "Missions" },
        };

        var res = new[] { building1, building2, mission }
            .AsQueryable()
            .TotalsByCategory(categories.AsQueryable(), gifts.AsQueryable())
            .ToList();

        Assert.Equal(["Missions", "Building"], res.Select(c => c.CategoryName));
        var building = res.Single(c => c.CategoryName == "Building");
        Assert.Equal((2, 1500m, 150m), (building.ProjectCount, building.TotalTarget, building.TotalRaised));
    }

    [Fact]
    public void RaisedInWindows_SplitsGiftsBetweenTheWindowAndTheOneBeforeIt()
    {
        var project = Proj(1m);
        var current = new DateWindow(new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 14), false);
        var previous = current.Previous();
        var gifts = new[]
        {
            Gift(project, 10m, new DateOnly(2026, 10, 5)),
            Gift(project, 20m, new DateOnly(2026, 10, 14)),
            Gift(project, 5m, new DateOnly(2026, 10, 4)),
            Gift(project, 7m, new DateOnly(2026, 9, 25)),
            Gift(project, 99m, new DateOnly(2026, 9, 24)),
            Gift(project, 99m, new DateOnly(2026, 10, 15)),
        };

        var res = gifts.AsQueryable().RaisedInWindows(current, previous).Single();

        Assert.Equal((30m, 12m), (res.Current, res.Previous));
    }

    [Fact]
    public void ByMonth_GroupsGiftsByTheMonthTheyWereGiven()
    {
        var project = Proj(1m);
        var gifts = new[]
        {
            Gift(project, 10m, new DateOnly(2026, 3, 1)),
            Gift(project, 15m, new DateOnly(2026, 3, 31)),
            Gift(project, 4m, new DateOnly(2026, 5, 9)),
        };

        var res = gifts.AsQueryable().ByMonth().OrderBy(r => r.Month).ToList();

        Assert.Equal(
            [new ContributionMonthRow(2026, 3, 25m, 2), new ContributionMonthRow(2026, 5, 4m, 1)],
            res
        );
    }

    [Fact]
    public void ContributionStats_From_TotalsCountsAveragesAndNamesTheMostUsedMethod()
    {
        var rows = new[]
        {
            new MethodTotal(PaymentMethod.Cash, 3, 300m),
            new MethodTotal(PaymentMethod.MobileMoney, 1, 100m),
        };

        var res = ContributionStats.From(rows);

        Assert.Equal((400m, 4, 100m, PaymentMethod.Cash), (res.Total, res.Count, res.Average, res.MostUsedPaymentMethod));
    }

    [Fact]
    public void ContributionStats_From_WithNoGifts_IsAllZeros()
    {
        var res = ContributionStats.From([]);

        Assert.Equal((0m, 0, 0m, (PaymentMethod?)null), (res.Total, res.Count, res.Average, res.MostUsedPaymentMethod));
    }

    [Fact]
    public void ContributionStats_From_BreaksATieByMethodSoTheAnswerIsStable()
    {
        var rows = new[]
        {
            new MethodTotal(PaymentMethod.MobileMoney, 2, 20m),
            new MethodTotal(PaymentMethod.Cash, 2, 20m),
        };

        Assert.Equal(PaymentMethod.Cash, ContributionStats.From(rows).MostUsedPaymentMethod);
    }

    [Fact]
    public void Projects_Paginate_Cursor_KeepsProjectsAfterItByStartDateThenId()
    {
        Project On(DateOnly start, string id)
        {
            var project = Proj(1m);
            project.StartDate = start;
            project.Id = Guid.Parse(id);
            return project;
        }

        var cursor = new ProjectCursor { StartDate = new DateOnly(2026, 3, 1), Id = Guid.Parse("00000000-0000-0000-0000-000000000005") };
        var before = On(new DateOnly(2026, 3, 1), "00000000-0000-0000-0000-000000000004");
        var after = On(new DateOnly(2026, 3, 1), "00000000-0000-0000-0000-000000000006");
        var earlier = On(new DateOnly(2026, 2, 1), "00000000-0000-0000-0000-000000000009");
        var later = On(new DateOnly(2026, 4, 1), "00000000-0000-0000-0000-000000000001");

        var res = new[] { before, after, earlier, later }.AsQueryable().Paginate(cursor, 10).ToList();

        Assert.Equal([after, later], res);
    }

    private static ProjectCategoryTotalDto Row(int id, string name, int projects, decimal target, decimal raised) =>
        new()
        {
            CategoryId = id,
            CategoryName = name,
            ProjectCount = projects,
            TotalTarget = target,
            TotalRaised = raised,
        };

    [Fact]
    public void WithOther_WhenThereAreFiveCategoriesOrFewer_ReturnsThemAsTheyAre()
    {
        var rows = Enumerable
            .Range(1, 5)
            .Select(i => Row(i, $"C{i}", 1, 100m, 500m - i))
            .ToList();

        var res = ProjectService.WithOther(rows);

        Assert.Equal(rows, res);
    }

    [Fact]
    public void WithOther_WhenThereAreMore_KeepsTheTopFourAndMergesTheRestIntoOtherLast()
    {
        var rows = new[]
        {
            Row(1, "Building", 3, 1000m, 900m),
            Row(2, "Missions", 2, 800m, 700m),
            Row(3, "Youth", 1, 500m, 400m),
            Row(4, "Music", 1, 300m, 250m),
            Row(5, "Welfare", 2, 200m, 150m),
            Row(6, "Media", 1, 100m, 60m),
            Row(7, "Garden", 4, 50m, 5m),
        };

        var res = ProjectService.WithOther(rows);

        Assert.Equal(5, res.Count);
        Assert.Equal(["Building", "Missions", "Youth", "Music", "Other"], res.Select(c => c.CategoryName));
        var other = res[^1];
        Assert.Null(other.CategoryId);
        Assert.Equal((7, 350m, 215m), (other.ProjectCount, other.TotalTarget, other.TotalRaised));
    }

    [Fact]
    public void WithOther_AddsUpToTheSameTotalsAsTheCategoriesItWasMadeFrom()
    {
        var rows = Enumerable
            .Range(1, 9)
            .Select(i => Row(i, $"C{i}", i, i * 10m, i * 3m))
            .ToList();

        var res = ProjectService.WithOther(rows);

        Assert.Equal(rows.Sum(r => r.ProjectCount), res.Sum(r => r.ProjectCount));
        Assert.Equal(rows.Sum(r => r.TotalTarget), res.Sum(r => r.TotalTarget));
        Assert.Equal(rows.Sum(r => r.TotalRaised), res.Sum(r => r.TotalRaised));
    }
}
