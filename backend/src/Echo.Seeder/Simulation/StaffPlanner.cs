using Echo.Domain.Users;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>
/// Decides how many people sign in to a congregation and in what role. The
/// five biggest congregations run with a team of 10 to 20 users, all the
/// others with 1 to 15. Every count includes the admin SetupPhase created.
/// </summary>
public static class StaffPlanner
{
    public const int StaffedCongregations = 5;

    private const int _staffedMin = 10;
    private const int _staffedMax = 20;
    private const int _smallMin = 1;
    private const int _smallMax = 15;

    // The first extra users take these roles in turn, so every role exists as
    // soon as a congregation has a few users. Anyone after that is drawn by weight.
    private static readonly UserRole[] _firstRoles =
    [
        UserRole.Accountant,
        UserRole.Clerk,
        UserRole.Member,
        UserRole.Admin,
    ];

    private static readonly IReadOnlyList<(UserRole Value, double Weight)> _weightedRoles =
    [
        (UserRole.Member, 45.0),
        (UserRole.Clerk, 30.0),
        (UserRole.Accountant, 20.0),
        (UserRole.Admin, 5.0),
    ];

    /// <summary>The roles of the users to add next to the admin.</summary>
    public static List<UserRole> ExtraRoles(Rng rng, bool staffed)
    {
        var total = staffed
            ? rng.Next(_staffedMin, _staffedMax + 1)
            : rng.Next(_smallMin, _smallMax + 1);
        var extra = total - 1;

        return
        [
            .. Enumerable
                .Range(0, extra)
                .Select(i =>
                    i < _firstRoles.Length ? _firstRoles[i] : rng.Weighted(_weightedRoles)
                ),
        ];
    }

    /// <summary>
    /// Sign-in names that are easy to remember: accountant1, clerk2, member3.
    /// The admin is admin, so a second admin is admin2.
    /// </summary>
    public static List<string> LocalParts(IReadOnlyList<UserRole> roles)
    {
        var seen = new Dictionary<UserRole, int>();
        var names = new List<string>(roles.Count);

        foreach (var role in roles)
        {
            seen[role] = seen.GetValueOrDefault(role) + 1;
            var number = role == UserRole.Admin ? seen[role] + 1 : seen[role];
            names.Add($"{role.ToString().ToLowerInvariant()}{number}");
        }

        return names;
    }
}
