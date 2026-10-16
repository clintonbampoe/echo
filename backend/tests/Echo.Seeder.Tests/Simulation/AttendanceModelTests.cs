using Echo.Domain.Attendances;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Simulation;

[Trait("Category", "Unit")]
public class AttendanceModelTests
{
    [Fact]
    public void ServiceChance_RisesWithCommitment_AndStaysBetweenZeroAndOne()
    {
        var previous = -1.0;

        for (var commitment = 0.0; commitment <= 1.0; commitment += 0.1)
        {
            var chance = AttendanceModel.ServiceChance(commitment);

            Assert.InRange(chance, 0.0, 1.0);
            Assert.True(chance > previous);
            previous = chance;
        }
    }

    [Fact]
    public void ServiceChance_MatchesTheDocumentedShape()
    {
        Assert.InRange(AttendanceModel.ServiceChance(0.1), 0.05, 0.15);
        Assert.InRange(AttendanceModel.ServiceChance(0.5), 0.35, 0.55);
        Assert.InRange(AttendanceModel.ServiceChance(1.0), 0.9, 1.0);
    }

    [Fact]
    public void BaseChance_StudyAndPrayerAreLowerThanServices()
    {
        var service = AttendanceModel.BaseChance(SessionCategory.Service, 0.6);

        Assert.True(AttendanceModel.BaseChance(SessionCategory.Study, 0.6) < service);
        Assert.True(AttendanceModel.BaseChance(SessionCategory.Prayer, 0.6) < service);
    }

    [Fact]
    public void BaseChance_SpecialDaysDrawMoreThanServices_ButNeverCertain()
    {
        Assert.True(
            AttendanceModel.BaseChance(SessionCategory.Special, 0.5)
                > AttendanceModel.BaseChance(SessionCategory.Service, 0.5)
        );
        Assert.True(AttendanceModel.BaseChance(SessionCategory.Special, 1.0) <= 0.97);
    }

    [Fact]
    public void Seasonal_EasterIsBusierThanAnOrdinarySunday()
    {
        var easter = Calendar.Easter(2026);
        var ordinary = new DateOnly(2026, 9, 20);

        Assert.True(
            AttendanceModel.Seasonal(easter, SessionCategory.Service)
                > AttendanceModel.Seasonal(ordinary, SessionCategory.Service)
        );
    }

    [Fact]
    public void Seasonal_ChristmasIsBusierThanAnOrdinarySunday()
    {
        Assert.True(
            AttendanceModel.Seasonal(new DateOnly(2026, 12, 25), SessionCategory.Service)
                > AttendanceModel.Seasonal(new DateOnly(2026, 9, 20), SessionCategory.Service)
        );
    }

    [Fact]
    public void Seasonal_MidweekGroupsFeelTheRainsMoreThanServices()
    {
        var inTheRains = new DateOnly(2026, 5, 20);

        Assert.True(
            AttendanceModel.Seasonal(inTheRains, SessionCategory.Study)
                < AttendanceModel.Seasonal(inTheRains, SessionCategory.Service)
        );
    }

    [Fact]
    public void FadeBeforeLeaving_IsOneForSomeoneWhoStays()
    {
        Assert.Equal(1.0, AttendanceModel.FadeBeforeLeaving(new DateOnly(2026, 1, 1), null));
    }

    [Fact]
    public void FadeBeforeLeaving_DropsInTheFourMonthsBeforeLeaving()
    {
        var left = new DateOnly(2026, 6, 1);

        var early = AttendanceModel.FadeBeforeLeaving(left.AddDays(-200), left);
        var month = AttendanceModel.FadeBeforeLeaving(left.AddDays(-30), left);
        var week = AttendanceModel.FadeBeforeLeaving(left.AddDays(-7), left);
        var lastDay = AttendanceModel.FadeBeforeLeaving(left, left);

        Assert.Equal(1.0, early);
        Assert.True(month < early);
        Assert.True(week < month);
        Assert.Equal(0.1, lastDay);
    }

    [Fact]
    public void NewcomerBoost_LastsEightWeeks()
    {
        var joined = new DateOnly(2026, 3, 1);

        Assert.Equal(1.25, AttendanceModel.NewcomerBoost(joined.AddDays(55), joined));
        Assert.Equal(1.0, AttendanceModel.NewcomerBoost(joined.AddDays(56), joined));
    }
}
