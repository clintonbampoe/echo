using Echo.Domain.Users;
using Echo.Seeder.Core;
using Echo.Seeder.Fakers;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Adds the staff who sign in next to each congregation's admin: accountants,
/// clerks, members and the odd second admin. See <see cref="StaffPlanner"/> for
/// the team sizes.
/// <br/><br/>
/// Runs after PeoplePhase because the biggest congregations (by member count)
/// get the big teams. Volume is a few hundred rows, so EF is fine.
/// <br/><br/>
/// Emails are <c>{role}{n}@{congregation}.org</c> (clerk1, accountant2, admin2),
/// all with the admin password.
/// </summary>
public sealed class UsersPhase(SeedFakers fakers) : ISeedPhase
{
    public string Name => "Users";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var staffed = ctx
            .State.OrderByDescending(s => s.Members.Count)
            .Take(StaffPlanner.StaffedCongregations)
            .Select(s => s.Id)
            .ToHashSet();

        var users = new List<User>();

        foreach (var seed in ctx.State)
        {
            var rng = ctx.RngFor(Name, seed.Congregation.Name);
            var roles = StaffPlanner.ExtraRoles(rng, staffed.Contains(seed.Id));
            var names = StaffPlanner.LocalParts(roles);
            var domain = seed.Admin.EmailAddress["admin@".Length..];

            for (var i = 0; i < roles.Count; i++)
                users.Add(MakeUser(rng, seed, roles[i], $"{names[i]}@{domain}", ctx.Now));

            ctx.Logger.LogInformation(
                "{Congregation}: {Count} users (admin included){Staffed}.",
                seed.Congregation.Name,
                roles.Count + 1,
                staffed.Contains(seed.Id) ? ", staffed" : ""
            );
        }

        await ctx.Db.Users.AddRangeAsync(users, ct);
        await ctx.Db.SaveChangesAsync(ct);
    }

    // Added some time after the admin set the congregation up; most verified their email.
    private User MakeUser(Rng rng, CongregationSeed seed, UserRole role, string email, DateTime now)
    {
        var user = fakers.User.Generate();
        var createdAt = rng.Between(seed.Admin.CreatedAt.AddDays(1), now.AddDays(-1));

        user.Id = rng.NewGuid();
        user.CongregationId = seed.Id;
        user.EmailAddress = email;
        user.Role = role;
        user.CreatedAt = createdAt;
        user.EmailVerifiedAt = rng.Chance(0.85) ? createdAt.AddMinutes(rng.Next(3, 600)) : null;

        return user;
    }
}
