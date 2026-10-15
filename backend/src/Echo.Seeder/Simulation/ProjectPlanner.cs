using Echo.Domain.Projects;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>A project and every contribution made to it.</summary>
public sealed record PlannedProject(
    Project Project,
    IReadOnlyList<ProjectContribution> Contributions
);

/// <summary>
/// Plans a congregation's projects and the money raised for each.
/// <br/><br/>
/// The status decides the story: a completed project ended in the past and
/// reached its target; one on track has raised about as much as the time
/// elapsed suggests; one at risk is well behind; a missed one ended short; a
/// project still in planning has not started raising money. Targets are sized
/// from the congregation's tithe income, so a small church does not run a
/// multi-million project.
/// <br/><br/>
/// Contributions are many gifts of varied size, clustered at the start and end
/// of a drive and on Sundays. Contributions are anonymous, as in the domain.
/// </summary>
public sealed class ProjectPlanner(
    CongregationSeed seed,
    Rng rng,
    FinanceWindow window,
    DateTime now,
    decimal monthlyIncome
)
{
    private const int _maxGiftsPerProject = 4_000;

    public List<PlannedProject> Build()
    {
        var categories = seed.ProjectCategories.ToDictionary(c => c.Name);
        var managers = ManagerPool();
        if (managers.Count == 0)
            return [];

        var count = seed.IsBig
            ? rng.Next(10, 17)
            : Math.Clamp(2 + seed.Members.Count / 150 + rng.Next(0, 2), 2, 6);

        var kinds = rng.Shuffle(ProjectCatalog.All)
            .Where(k => categories.ContainsKey(k.Category))
            .Take(count)
            .ToList();

        return [.. kinds.Select(k => Plan(k, categories[k.Category], managers))];
    }

    // Committed, active adults, with the leaders of ministries first in line.
    private List<Guid> ManagerPool()
    {
        var leaders = seed.Organizations.Select(o => o.LeaderId).ToHashSet();

        return
        [
            .. seed
                .Members.Where(m => m.LeftOn is null && m.AgeOn(window.Today) >= 28)
                .OrderByDescending(m =>
                    m.Commitment + (leaders.Contains(m.Id) ? 0.25 : 0.0) + rng.Gaussian(0, 0.05)
                )
                .Take(40)
                .Select(m => m.Id),
        ];
    }

    private PlannedProject Plan(ProjectKind kind, ProjectCategory category, List<Guid> managers)
    {
        var today = window.Today;
        var status = rng.Weighted<ProjectStatus>([
            (ProjectStatus.Planning, 10.0),
            (ProjectStatus.OnTrack, 30.0),
            (ProjectStatus.AtRisk, 15.0),
            (ProjectStatus.Complete, 38.0),
            (ProjectStatus.Missed, 7.0),
        ]);

        var target = Money.RoundNicely(
            (double)monthlyIncome * rng.Range(kind.MinMonths, kind.MaxMonths)
        );

        // Bigger projects take longer, but never longer than the data goes back.
        var horizonDays = today.DayNumber - window.Start.DayNumber;
        var months = Math.Clamp(
            (int)(4 + Math.Log(1 + kind.MaxMonths) * 9) + rng.Next(-2, 6),
            4,
            48
        );
        months = Math.Max(3, Math.Min(months, (horizonDays - 60) / 30));
        var days = months * 30;

        var (start, end) = Timeline(status, today, days, horizonDays);

        var elapsed =
            end <= start
                ? 1.0
                : Math.Clamp(
                    (today.DayNumber - start.DayNumber) / (double)(end.DayNumber - start.DayNumber),
                    0.0,
                    1.0
                );

        var funded = status switch
        {
            ProjectStatus.Complete => rng.Range(1.0, 1.12),
            ProjectStatus.OnTrack => elapsed * rng.Range(0.9, 1.1),
            ProjectStatus.AtRisk => elapsed * rng.Range(0.3, 0.6),
            ProjectStatus.Missed => rng.Range(0.35, 0.85),
            _ => 0.0,
        };

        var project = new Project
        {
            Id = rng.NewGuid(),
            CongregationId = seed.Id,
            CategoryId = category.Id,
            ManagerId = rng.Pick(managers),
            Name = kind.Name,
            TargetAmount = target,
            Status = status,
            StartDate = start,
            // A drive still running is sometimes left open-ended.
            EndDate =
                status is ProjectStatus.Complete or ProjectStatus.Missed || rng.Chance(0.9)
                    ? end
                    : null,
            Description = kind.Description,
            CreatedAt = Stamps.On(
                (start < today ? start : today).AddDays(-rng.Next(7, 30)),
                rng,
                now
            ),
        };

        var windowEnd = status is ProjectStatus.OnTrack or ProjectStatus.AtRisk
            ? today
            : (end < today ? end : today);

        List<ProjectContribution> contributions =
            status == ProjectStatus.Planning
                ? []
                : Contributions(project, (decimal)((double)target * funded), start, windowEnd);

        return new PlannedProject(project, contributions);
    }

    // When the project starts and ends, given how it is going.
    private (DateOnly Start, DateOnly End) Timeline(
        ProjectStatus status,
        DateOnly today,
        int days,
        int horizonDays
    )
    {
        DateOnly start;
        DateOnly end;

        switch (status)
        {
            case ProjectStatus.Complete:
            case ProjectStatus.Missed:
                var oldest =
                    status == ProjectStatus.Missed
                        ? Math.Min(400, horizonDays - days)
                        : horizonDays - days;
                end = today.AddDays(-rng.Next(30, Math.Max(31, oldest)));
                start = end.AddDays(-days);
                break;

            case ProjectStatus.Planning:
                start = today.AddDays(rng.Next(14, 120));
                end = start.AddDays(days);
                break;

            default:
                start = today.AddDays(-(int)(days * rng.Range(0.2, 0.85)));
                end = start.AddDays(days);
                break;
        }

        if (start < window.Start)
            start = window.Start;

        if (end <= start)
            end = start.AddDays(days);

        // A project still running must end in the future.
        if (status is ProjectStatus.OnTrack or ProjectStatus.AtRisk && end <= today)
            end = today.AddDays(30);

        return (start, end);
    }

    // Many gifts of varied size that add up to the amount raised.
    private List<ProjectContribution> Contributions(
        Project project,
        decimal raised,
        DateOnly from,
        DateOnly to
    )
    {
        var gifts = new List<ProjectContribution>();
        var median = Math.Clamp((double)project.TargetAmount / 700.0, 40.0, 3000.0);
        var remaining = raised;

        for (var i = 0; remaining >= 10m && i < _maxGiftsPerProject; i++)
        {
            var amount = Math.Min(Money.RoundLikeAPerson(rng.LogNormal(median, 0.9)), remaining);
            remaining -= amount;

            var date = GiftDate(from, to);

            gifts.Add(
                new ProjectContribution
                {
                    Id = rng.NewGuid(),
                    CongregationId = seed.Id,
                    ProjectId = project.Id,
                    Amount = amount,
                    DateContributed = date,
                    PaymentMethod = GivingFactory.PickAnyMethod(rng),
                    Description = rng.Chance(0.12) ? rng.Pick(_gestures) : null,
                    CreatedAt = Stamps.On(
                        date.AddDays(date.DayOfWeek == DayOfWeek.Sunday ? 1 : 0),
                        rng,
                        now
                    ),
                }
            );
        }

        return gifts;
    }

    // Drives start and end strong, and people mostly give on Sundays.
    private DateOnly GiftDate(DateOnly from, DateOnly to)
    {
        var span = Math.Max(0, to.DayNumber - from.DayNumber);
        var u = rng.NextDouble();
        var position = rng.Chance(0.5) ? u : (rng.Chance(0.5) ? u * u : 1.0 - u * u);
        var date = from.AddDays((int)(position * span));

        if (rng.Chance(0.6))
        {
            var sunday = Calendar.OnOrAfter(date, DayOfWeek.Sunday);
            if (sunday <= to)
                date = sunday;
        }

        return date;
    }

    private static readonly string[] _gestures =
    [
        "Pledge redemption",
        "Harvest contribution",
        "Fulfilment of pledge",
        "Special offering",
        "In memory of a loved one",
    ];
}
