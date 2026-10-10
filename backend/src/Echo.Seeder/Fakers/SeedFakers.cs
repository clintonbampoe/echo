using Bogus;
using Echo.Domain.Members;
using Echo.Domain.Users;

namespace Echo.Seeder.Fakers;

/// <summary>
/// Pre-built Bogus fakers for each entity type.
///
/// All fakers are seeded from the global seed so every run produces identical
/// data. Bogus uses its own internal sequence — passing the same seed to
/// UseSeed() always produces the same sequence of values.
///
/// We use the "en" locale. This gives realistic western names and addresses.
/// The goal is production-like data (realistic distributions, plausible
/// values) not a replica of any specific real-world population.
///
/// One SeedFakers instance is created per seeder run and shared across phases.
/// Do not share a single Faker<T> across threads.
/// </summary>
public sealed class SeedFakers
{
    public Faker<Domain.Members.Person> Person { get; }
    public Faker<Member> Member { get; }
    public Faker<User> User { get; }

    // Raw Faker for one-off values (emails, phones, addresses) not tied to an entity
    public Faker Raw { get; }

    public SeedFakers(int seed)
    {
        Raw = new Faker("en") { Random = new Randomizer(seed) };

        // Person: shared columns only (Kind, FirstName, LastName, OtherNames,
        // PhoneNumber, EmailAddress). The rest of the identity lives on Member
        // or Visitor side tables.
        Person = new Faker<Domain.Members.Person>("en")
            .UseSeed(seed)
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            // CongregationId set by the phase
            .RuleFor(p => p.Kind, _ => PersonKind.Member) // overridden per call
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.OtherNames, f => f.Random.Bool(0.3f) ? f.Name.FirstName() : null)
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber("0##-###-####"))
            .RuleFor(
                p => p.EmailAddress,
                (f, p) => f.Random.Bool(0.6f) ? f.Internet.Email(p.FirstName, p.LastName) : null
            )
            .RuleFor(p => p.CreatedAt, _ => DateTime.MinValue); // set by phase

        // Member: the side table that hangs off Person.
        // PersonId and CongregationId set by the phase.
        Member = new Faker<Member>("en")
            .UseSeed(seed)
            .RuleFor(
                m => m.DateOfBirth,
                f => DateOnly.FromDateTime(f.Date.Past(62, DateTime.Now.AddYears(-18)))
            )
            .RuleFor(m => m.JoinedDate, f => DateOnly.FromDateTime(f.Date.Past(10)))
            .RuleFor(m => m.Gender, f => f.PickRandom<Gender>())
            .RuleFor(m => m.ResidentialAddress, f => f.Address.StreetAddress())
            .RuleFor(m => m.City, f => f.Address.City())
            .RuleFor(m => m.Hometown, f => f.Address.City())
            .RuleFor(m => m.Region, f => f.PickRandom<Region>())
            .RuleFor(
                m => m.GpsAddress,
                f => f.Random.Bool(0.7f) ? f.Address.Latitude().ToString("F6") : null
            )
            .RuleFor(m => m.MaritalStatus, f => f.PickRandom<MaritalStatus>())
            .RuleFor(m => m.NextOfKin, f => f.Name.FullName())
            .RuleFor(m => m.EmergencyContactName, f => f.Name.FullName())
            .RuleFor(m => m.EmergencyContactPhoneNumber, f => f.Phone.PhoneNumber("0##-###-####"))
            .RuleFor(
                m => m.Status,
                f =>
                    f.Random.WeightedRandom(
                        [
                            MemberStatus.Active,
                            MemberStatus.Inactive,
                            MemberStatus.Archived,
                            MemberStatus.Transferred,
                        ],
                        [0.92f, 0.05f, 0.02f, 0.01f]
                    )
            )
            .RuleFor(m => m.CreatedAt, _ => DateTime.MinValue); // set by phase

        User = new Faker<User>("en")
            .UseSeed(seed)
            .RuleFor(u => u.Id, _ => Guid.NewGuid())
            // CongregationId set by phase
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.OtherNames, _ => null)
            .RuleFor(
                u => u.EmailAddress,
                (f, u) =>
                    $"admin.{u.FirstName!.ToLower()}.{u.LastName!.ToLower()}@{f.Internet.DomainName()}"
            )
            // A real bcrypt hash of the literal string "Password1!".
            // All dev admin accounts use this password.
            .RuleFor(
                u => u.PasswordHash,
                _ => "$2a$10$kCprmXr4NuNu4RsweXparO/.mabtwE/zzRUmS3u/KtkoKE0GiYeQu"
            )
            .RuleFor(u => u.Role, _ => UserRole.Admin)
            .RuleFor(
                u => u.EmailVerifiedAt,
                f => DateTime.SpecifyKind(f.Date.Past(2), DateTimeKind.Utc)
            )
            .RuleFor(u => u.CreatedAt, _ => DateTime.MinValue); // set by phase
    }
}
