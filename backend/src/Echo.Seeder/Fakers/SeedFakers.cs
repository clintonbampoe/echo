using Bogus;
using Echo.Domain.Users;

namespace Echo.Seeder.Fakers;

/// <summary>
/// Bogus fakers for the entities that are not part of the people simulation.
///
/// People are built by <c>Simulation/PeopleFactory</c>, which needs households
/// and join dates that agree with each other. A per-field faker cannot do that.
///
/// <br/><br/>
/// Both fakers are seeded from the global seed, so every run gives identical
/// data. The user faker only makes names, the password and the role; ids, email
/// and dates come from SetupPhase, which knows the congregation. One SeedFakers
/// instance is created per seeder run and shared across phases. Do not share a
/// single Faker across threads.
/// </summary>
public sealed class SeedFakers
{
    public Faker<User> User { get; }

    // Raw Faker for one-off values (cities, names of places) not tied to an entity
    public Faker Raw { get; }

    public SeedFakers(int seed)
    {
        Raw = new Faker("en") { Random = new Randomizer(seed) };

        User = new Faker<User>("en")
            .UseSeed(seed)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.OtherNames, _ => null)
            // A real bcrypt hash of the literal string "Password1!".
            // All dev admin accounts use this password.
            .RuleFor(
                u => u.PasswordHash,
                _ => "$2a$10$7L3PEPyl.S5oMOGZI8sJge9reZ4VB1Y8.KuAyGLF3spikGluNEXo6"
            )
            .RuleFor(u => u.Role, _ => UserRole.Admin);
    }
}
