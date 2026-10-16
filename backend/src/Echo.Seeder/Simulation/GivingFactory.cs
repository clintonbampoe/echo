using Echo.Domain.Transactions;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

public enum IncomeBand
{
    Low,
    Middle,
    High,
}

/// <summary>
/// Decides how a person gives: how often, how much and how they pay.
/// </summary>
public static class GivingFactory
{
    private static readonly IReadOnlyList<(IncomeBand Value, double Weight)> _bands =
    [
        (IncomeBand.Low, 50.0),
        (IncomeBand.Middle, 38.0),
        (IncomeBand.High, 12.0),
    ];

    public static IncomeBand PickBand(Rng rng) => rng.Weighted(_bands);

    /// <summary>A payment method for someone whose income band is not known, such as an anonymous gift.</summary>
    public static PaymentMethod PickAnyMethod(Rng rng) => PickMethod(rng, IncomeBand.Middle);

    public static Giving Create(Rng rng, int age, double commitment, IncomeBand band)
    {
        if (age < 18)
            return new Giving(GivingStyle.None, 0m, PaymentMethod.Cash);

        return new Giving(
            PickStyle(rng, commitment),
            PickAmount(rng, age, band),
            PickMethod(rng, band)
        );
    }

    // A committed person is more likely to be faithful; an uncommitted one is
    // more likely not to tithe at all.
    private static GivingStyle PickStyle(Rng rng, double commitment)
    {
        var none = 0.30 - 0.25 * commitment;
        var occasional = 0.20;
        var faithful = 0.10 + 0.45 * commitment;

        var roll = rng.NextDouble();
        if (roll < none)
            return GivingStyle.None;
        if (roll < none + occasional)
            return GivingStyle.Occasional;
        if (roll > 1.0 - faithful)
            return GivingStyle.Faithful;
        return GivingStyle.Regular;
    }

    // Money amounts are log-normal: most people give near the median for their
    // income, a few give far more. Students and young workers give less.
    private static decimal PickAmount(Rng rng, int age, IncomeBand band)
    {
        var median = band switch
        {
            IncomeBand.Low => 40.0,
            IncomeBand.Middle => 150.0,
            _ => 550.0,
        };

        if (age < 25)
            median *= 0.5;

        return Money.RoundLikeAPerson(rng.LogNormal(median, 0.45));
    }

    private static PaymentMethod PickMethod(Rng rng, IncomeBand band) =>
        band switch
        {
            IncomeBand.Low => rng.Weighted<PaymentMethod>([
                (PaymentMethod.Cash, 55.0),
                (PaymentMethod.MobileMoney, 40.0),
                (PaymentMethod.BankTransfer, 3.0),
                (PaymentMethod.Cheque, 1.0),
                (PaymentMethod.CreditCard, 1.0),
            ]),
            IncomeBand.Middle => rng.Weighted<PaymentMethod>([
                (PaymentMethod.Cash, 35.0),
                (PaymentMethod.MobileMoney, 45.0),
                (PaymentMethod.BankTransfer, 15.0),
                (PaymentMethod.Cheque, 3.0),
                (PaymentMethod.CreditCard, 2.0),
            ]),
            _ => rng.Weighted<PaymentMethod>([
                (PaymentMethod.Cash, 15.0),
                (PaymentMethod.MobileMoney, 35.0),
                (PaymentMethod.BankTransfer, 35.0),
                (PaymentMethod.Cheque, 8.0),
                (PaymentMethod.CreditCard, 7.0),
            ]),
        };
}
