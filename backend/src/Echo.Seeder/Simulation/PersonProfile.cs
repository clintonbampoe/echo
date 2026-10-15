using Echo.Domain.Members;
using Echo.Domain.Transactions;

namespace Echo.Seeder.Simulation;

/// <summary>
/// How a member gives over time. Later phases turn this into tithe rows.
/// </summary>
public enum GivingStyle
{
    /// <summary>Never tithes (children, and some adults).</summary>
    None,

    /// <summary>Gives in some months, skips others.</summary>
    Occasional,

    /// <summary>Gives in most months.</summary>
    Regular,

    /// <summary>Gives every month they are a member.</summary>
    Faithful,
}

/// <param name="Style">How often they give.</param>
/// <param name="MonthlyAmount">What they usually give, a round figure like a real person's.</param>
/// <param name="Method">The way they usually pay.</param>
public sealed record Giving(GivingStyle Style, decimal MonthlyAmount, PaymentMethod Method);

/// <summary>
/// The simulated life of one member, kept in memory for the whole run so that
/// attendance, tithes, organizations and events can all agree with it.
/// </summary>
public sealed class PersonProfile
{
    public required Guid Id { get; init; }
    public required Guid HouseholdId { get; init; }
    public required Gender Gender { get; init; }
    public required DateOnly DateOfBirth { get; init; }

    /// <summary>When they really joined, even if the member record leaves JoinedDate empty.</summary>
    public required DateOnly JoinedOn { get; init; }

    /// <summary>When they stopped coming. Null while they are still around.</summary>
    public DateOnly? LeftOn { get; init; }

    public required MemberStatus Status { get; init; }

    /// <summary>
    /// 0 to 1. How committed this person is to the congregation. Drives how
    /// often they attend and how likely they are to give faithfully. Children
    /// inherit their household's value, since they come with their parents.
    /// </summary>
    public required double Commitment { get; init; }

    public required Giving Giving { get; init; }

    /// <summary>True while the person belongs to the congregation on that day.</summary>
    public bool IsMemberOn(DateOnly date) => date >= JoinedOn && (LeftOn is null || date < LeftOn);

    public int AgeOn(DateOnly date)
    {
        var age = date.Year - DateOfBirth.Year;
        return date < DateOfBirth.AddYears(age) ? age - 1 : age;
    }
}

/// <summary>
/// One visitor's story. A converted visitor also has a <see cref="PersonProfile"/>
/// under the same id, from the day they became a member.
/// </summary>
public sealed record VisitorProfile(Guid Id, DateOnly FirstVisit, DateOnly? ConvertedOn);
