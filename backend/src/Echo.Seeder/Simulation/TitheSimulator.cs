using Echo.Domain.Tithes;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>
/// Produces every tithe for one congregation, member by member.
/// <br/><br/>
/// A tithe follows the member's <see cref="Giving"/> profile: faithful givers
/// give nearly every month, occasional ones give now and then. Nobody gives
/// before they joined or after they left, and people give less as they drift
/// away. Each member has at most one tithe per month, as the database requires.
/// <br/><br/>
/// What a tithe looks like: the amount is the member's usual figure, adjusted
/// for how long ago it was (older tithes were smaller), with the odd bigger or
/// smaller month, a Christmas bump, and a double tithe after a missed month.
/// Most are given at a Sunday service; the rest when someone is paid, and a few
/// arrive in the first week of the following month. People pay the way they
/// usually pay, most of the time.
/// </summary>
public sealed class TitheSimulator(
    CongregationSeed seed,
    Rng rng,
    FinanceWindow window,
    DateTime now
)
{
    /// <summary>
    /// The total given on each collection date. Complete once
    /// <see cref="Generate"/> has been fully enumerated; the ledger uses it so
    /// the transactions add up to the tithes.
    /// </summary>
    public Dictionary<DateOnly, decimal> TotalsByDate { get; } = [];

    /// <summary>How many tithes have been produced so far.</summary>
    public int Count { get; private set; }

    public IEnumerable<Tithe> Generate()
    {
        foreach (var member in seed.Members)
        {
            if (member.Giving.Style == GivingStyle.None)
                continue;

            foreach (var tithe in TithesOf(member))
            {
                TotalsByDate[tithe.CollectionDate] =
                    TotalsByDate.GetValueOrDefault(tithe.CollectionDate) + tithe.Amount;

                Count++;
                yield return tithe;
            }
        }
    }

    private IEnumerable<Tithe> TithesOf(PersonProfile member)
    {
        var giving = member.Giving;
        var skippedLast = false;
        var joinedMonth = new DateOnly(member.JoinedOn.Year, member.JoinedOn.Month, 1);

        for (
            var month = Later(window.FirstMonth, joinedMonth);
            month <= window.LastMonth;
            month = month.AddMonths(1)
        )
        {
            // No tithe for a month that has not happened yet, or that they were not around for.
            if (CollectionDate(month) is not { } date || !member.IsMemberOn(date))
                continue;

            var chance =
                MonthlyChance(giving.Style, skippedLast)
                * AttendanceModel.FadeBeforeLeaving(date, member.LeftOn);

            if (!rng.Chance(chance))
            {
                skippedLast = true;
                continue;
            }

            var factor = rng.Chance(0.2) ? rng.Range(0.6, 1.4) : 1.0;
            string? description = null;

            if (skippedLast && giving.Style != GivingStyle.Occasional && rng.Chance(0.4))
            {
                // Making up for the month they missed.
                var missed = (MonthOfYear)month.AddMonths(-1).Month;
                factor *= 2;
                description = $"Includes {missed}";
            }
            else if (month.Month == 12 && rng.Chance(0.45))
            {
                factor *= 1.5;
                description = "Christmas thanksgiving";
            }

            skippedLast = false;

            // The same person gave less a few years ago.
            var yearsAgo = (window.Today.DayNumber - date.DayNumber) / 365.25;
            var amount = Money.RoundLikeAPerson(
                (double)giving.MonthlyAmount * Math.Pow(0.92, yearsAgo) * factor
            );

            yield return new Tithe
            {
                Id = rng.NewGuid(),
                CongregationId = seed.Id,
                MemberId = member.Id,
                Amount = amount,
                ForYear = month.Year,
                ForMonth = (MonthOfYear)month.Month,
                PaymentMethod = rng.Chance(0.9) ? giving.Method : GivingFactory.PickAnyMethod(rng),
                CollectionDate = date,
                Description = description,
                // Recorded by the admin the next day if it was given at a service.
                CreatedAt = Stamps.On(
                    date.AddDays(date.DayOfWeek == DayOfWeek.Sunday ? 1 : 0),
                    rng,
                    now
                ),
            };
        }
    }

    private static double MonthlyChance(GivingStyle style, bool skippedLast)
    {
        var chance = style switch
        {
            GivingStyle.Faithful => 0.97,
            GivingStyle.Regular => 0.85,
            _ => 0.35,
        };

        return skippedLast && style != GivingStyle.Occasional
            ? Math.Min(0.98, chance + 0.1)
            : chance;
    }

    // The day a month's tithe was given, or null if that day has not come yet.
    private DateOnly? CollectionDate(DateOnly month)
    {
        DateOnly date;

        if (rng.Chance(0.06))
        {
            var next = month.AddMonths(1);
            date = new DateOnly(next.Year, next.Month, rng.Next(1, 8));
        }
        else if (rng.Chance(0.75))
        {
            var sunday = rng.Weighted<int>([(1, 40.0), (2, 25.0), (3, 20.0), (4, 15.0)]);
            date = Calendar.NthWeekday(month.Year, month.Month, DayOfWeek.Sunday, sunday);
        }
        else
        {
            var day = rng.Next(1, DateTime.DaysInMonth(month.Year, month.Month) + 1);
            date = new DateOnly(month.Year, month.Month, day);
        }

        return date <= window.Today ? date : null;
    }

    private static DateOnly Later(DateOnly a, DateOnly b) => a > b ? a : b;
}
