using Echo.Domain.Attendances;
using Echo.Domain.Transactions;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>
/// Builds a congregation's transactions so that they agree with everything
/// else that was generated.
/// <br/><br/>
/// Income: the tithes received on each collection date (the same totals as the
/// tithe rows), the Sunday offering (people at the service times what they
/// usually put in), harvest and convention appeals, project contributions by
/// month, and the odd donation. Expenses follow income: salaries, utilities,
/// rent for the smaller congregations, repairs, transport, events, outreach,
/// printing and hospitality, each as a share of that month's income.
/// </summary>
public sealed class LedgerBuilder(
    CongregationSeed seed,
    Rng rng,
    FinanceWindow window,
    DateTime now,
    IReadOnlyDictionary<DateOnly, decimal> titheTotals,
    IReadOnlyList<PlannedProject> projects
)
{
    private sealed record Expense(
        string Category,
        double Share,
        double Chance,
        int Day,
        string[] Descriptions
    );

    private static readonly Expense[] _expenses =
    [
        new("Salaries", 0.30, 1.0, 28, ["Pastoral and staff salaries"]),
        new("Utilities", 0.025, 1.0, 12, ["Electricity bill"]),
        new("Utilities", 0.006, 0.9, 14, ["Water bill"]),
        new("Utilities", 0.004, 0.5, 16, ["Internet and data subscription"]),
        new(
            "Maintenance",
            0.06,
            0.45,
            20,
            [
                "Repairs to the sanctuary",
                "Generator servicing",
                "Plumbing repairs",
                "Painting and touch-ups",
                "Sound system repairs",
                "Roof leak repairs",
            ]
        ),
        new(
            "Transportation",
            0.015,
            0.8,
            18,
            ["Fuel for the church bus", "Transport for visiting ministers", "Vehicle servicing"]
        ),
        new(
            "Events",
            0.07,
            0.5,
            15,
            [
                "Youth camp expenses",
                "Hall hire for the conference",
                "Equipment hire for the anniversary",
                "Seminar materials and refreshments",
            ]
        ),
        new(
            "Outreach",
            0.06,
            0.45,
            22,
            [
                "Food items for the outreach",
                "Medical outreach supplies",
                "Support for a needy family",
                "Evangelism materials",
            ]
        ),
        new(
            "Printing",
            0.008,
            0.5,
            8,
            ["Printing of programmes", "Banners and posters", "Printing of membership forms"]
        ),
        new(
            "Hospitality",
            0.012,
            0.65,
            24,
            [
                "Refreshments for visiting ministers",
                "Water and snacks for the service",
                "Catering for the leaders' meeting",
            ]
        ),
    ];

    private static readonly string[] _donations =
    [
        "Donation from a well-wisher",
        "Donation from a member",
        "Anonymous donation",
        "Donation in memory of a loved one",
        "Donation from a visiting family",
    ];

    private readonly Dictionary<string, TransactionCategory> _categories =
        seed.TransactionCategories.ToDictionary(c => c.Name);

    private readonly List<Transaction> _rows = [];

    public List<Transaction> Build()
    {
        AddTithes();
        AddOfferings();
        AddAppeals();
        AddProjectIncome();
        AddDonations();
        AddExpenses();

        return [.. _rows.OrderBy(r => r.TransactionDate)];
    }

    // ── Income ────────────────────────────────────────────────────────────

    private void AddTithes()
    {
        foreach (var (date, total) in titheTotals.OrderBy(t => t.Key))
        {
            Add(
                "Tithes",
                date,
                total,
                date.DayOfWeek == DayOfWeek.Sunday
                    ? "Tithes received at the service"
                    : "Tithes received"
            );
        }
    }

    private void AddOfferings()
    {
        foreach (var sunday in window.Sundays())
        {
            var perPerson = rng.LogNormal(seed.IsBig ? 7.0 : 5.0, 0.25);
            var total = ExpectedAttendance(sunday) * perPerson * OfferingFactor(sunday);

            if (seed.IsBig)
            {
                var first = rng.Range(0.42, 0.5);
                Add("Offerings", sunday, Money.Cents(total * first), "First service offering");
                Add(
                    "Offerings",
                    sunday,
                    Money.Cents(total * (1 - first)),
                    "Second service offering"
                );
            }
            else
            {
                Add("Offerings", sunday, Money.Cents(total), "Sunday service offering");
            }
        }
    }

    // The harvest thanksgiving appeal and the annual convention.
    private void AddAppeals()
    {
        for (var year = window.Start.Year; year <= window.Today.Year; year++)
        {
            var harvest = Calendar.NthWeekday(year, 11, DayOfWeek.Sunday, 1);
            Add(
                "Fundraising",
                harvest,
                Money.Cents(ExpectedAttendance(harvest) * rng.LogNormal(30.0, 0.3)),
                "Harvest thanksgiving proceeds"
            );

            var convention = Calendar.NthWeekday(year, 8, DayOfWeek.Saturday, 3);
            Add(
                "Offerings",
                convention,
                Money.Cents(ExpectedAttendance(convention) * 0.6 * rng.LogNormal(15.0, 0.3)),
                "Annual convention offering"
            );
        }
    }

    // What members put towards each project, one row per project per month.
    private void AddProjectIncome()
    {
        foreach (var planned in projects)
        {
            var byMonth = planned.Contributions.GroupBy(c =>
                (c.DateContributed.Year, c.DateContributed.Month)
            );

            foreach (var month in byMonth)
            {
                Add(
                    "Pledges",
                    month.Max(c => c.DateContributed),
                    month.Sum(c => c.Amount),
                    $"Contributions to {planned.Project.Name}"
                );
            }
        }
    }

    private void AddDonations()
    {
        var titheByMonth = TotalsByMonth(titheTotals.Select(t => (t.Key, t.Value)));

        foreach (var month in window.Months())
        {
            var basis = (double)titheByMonth.GetValueOrDefault((month.Year, month.Month));
            var gifts = rng.Weighted<int>([(0, 40.0), (1, 35.0), (2, 20.0), (3, 5.0)]);

            for (var i = 0; i < gifts; i++)
            {
                var day = rng.Next(1, DateTime.DaysInMonth(month.Year, month.Month) + 1);
                Add(
                    "Donations",
                    new DateOnly(month.Year, month.Month, day),
                    Money.RoundNicely(basis * rng.LogNormal(0.004, 0.9)),
                    rng.Pick(_donations)
                );
            }
        }
    }

    // ── Expenses ──────────────────────────────────────────────────────────

    private void AddExpenses()
    {
        var income = TotalsByMonth(
            _rows
                .Where(r => r.TransactionType == TransactionType.Income)
                .Select(r => (r.TransactionDate, r.Amount))
        );

        // Some of the smaller congregations rent their hall, at a fixed amount.
        var completed = income
            .Where(i => (i.Key.Year, i.Key.Month) != (window.Today.Year, window.Today.Month))
            .ToList();
        var rent =
            !seed.IsBig && rng.Chance(0.5) && completed.Count > 0
                ? Money.RoundNicely((double)completed.Average(i => i.Value) * 0.1)
                : 0m;

        foreach (var month in window.Months())
        {
            var monthly = (double)income.GetValueOrDefault((month.Year, month.Month));
            if (monthly <= 0)
                continue;

            Add("Rent", month, rent, "Monthly rent for the hall");

            var days = DateTime.DaysInMonth(month.Year, month.Month);

            foreach (var expense in _expenses)
            {
                if (!rng.Chance(expense.Chance))
                    continue;

                var day = Math.Clamp(expense.Day + rng.Next(-3, 4), 1, days);
                Add(
                    expense.Category,
                    new DateOnly(month.Year, month.Month, day),
                    Money.RoundNicely(monthly * expense.Share * rng.LogNormal(1.0, 0.25)),
                    rng.Pick(expense.Descriptions)
                );
            }
        }
    }

    // ── Shared ────────────────────────────────────────────────────────────

    // How many people are expected on a day, from the same rules attendance uses.
    private double ExpectedAttendance(DateOnly date)
    {
        var seasonal = AttendanceModel.Seasonal(date, SessionCategory.Service);
        var total = 0.0;

        foreach (var member in seed.Members)
        {
            if (member.IsMemberOn(date))
                total += AttendanceModel.ServiceChance(member.Commitment);
        }

        return total * seasonal;
    }

    // Harvest Sunday is the biggest offering of the year.
    private static double OfferingFactor(DateOnly date) =>
        date == Calendar.NthWeekday(date.Year, 11, DayOfWeek.Sunday, 1) ? 2.5 : 1.0;

    private static Dictionary<(int Year, int Month), decimal> TotalsByMonth(
        IEnumerable<(DateOnly Date, decimal Amount)> rows
    ) =>
        rows.GroupBy(r => (r.Date.Year, r.Date.Month))
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));

    private void Add(string category, DateOnly date, decimal amount, string description)
    {
        if (
            amount <= 0m
            || date > window.Today
            || date < window.Start
            || !_categories.TryGetValue(category, out var found)
        )
            return;

        _rows.Add(
            new Transaction
            {
                Id = rng.NewGuid(),
                CongregationId = seed.Id,
                CategoryId = found.Id,
                TransactionType = found.CategoryType,
                TransactionDate = date,
                Amount = amount,
                Description = description,
                CreatedAt = Stamps.On(
                    date.AddDays(date.DayOfWeek == DayOfWeek.Sunday ? 1 : 0),
                    rng,
                    now
                ),
            }
        );
    }
}
