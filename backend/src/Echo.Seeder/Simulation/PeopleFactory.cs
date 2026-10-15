using Bogus;
using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Seeder.Core;
using BogusGender = Bogus.DataSets.Name.Gender;
using Person = Echo.Domain.Members.Person;

namespace Echo.Seeder.Simulation;

/// <summary>A member's rows plus the profile later phases use.</summary>
public sealed record NewMember(Person Person, Member Member, PersonProfile Profile);

/// <summary>
/// A visitor's rows. A visitor who converted also has a Member row and a profile.
/// </summary>
public sealed record NewVisitor(
    Person Person,
    Visitor Visitor,
    Member? Member,
    PersonProfile? Profile,
    VisitorProfile Story
);

/// <summary>
/// Builds people the way a congregation really gets them: in households, on
/// the days they joined, with names, addresses and phone numbers that agree
/// with each other.
/// <br/><br/>
/// A household shares a surname, an address and a join date. Children carry
/// their parent's phone number and have no email. Some households later leave;
/// when they do, every member leaves with them. Visitors arrive over time and
/// some of them become members.
/// </summary>
public sealed class PeopleFactory(
    Rng rng,
    Faker faker,
    Congregation congregation,
    GrowthCurve growth,
    DateTime now
)
{
    private readonly DateOnly _today = DateOnly.FromDateTime(now);

    private enum Role
    {
        Head,
        Spouse,
        Child,
    }

    private sealed record Seat(Role Role, Gender Gender, DateOnly DateOfBirth, DateOnly JoinedOn);

    private sealed record Residence(
        string Address,
        string City,
        string Hometown,
        Region Region,
        string? GpsAddress
    );

    private sealed record Household(
        Guid Id,
        DateOnly JoinedOn,
        double Commitment,
        IncomeBand Income,
        DateOnly? LeftOn,
        MemberStatus Status,
        Residence Residence,
        string LastName
    );

    // ── Households ────────────────────────────────────────────────────────

    public IReadOnlyList<NewMember> BuildHousehold()
    {
        var joinedOn = growth.SampleJoinDate(rng);
        var commitment = Math.Clamp(rng.Gaussian(0.55, 0.24), 0.03, 0.99);
        var household = NewHousehold(joinedOn, commitment, faker.Name.LastName());

        var size = rng.Weighted(_householdSizes);
        var adults = AdultsIn(size);
        var seats = PlanSeats(household, adults, size - adults);

        var couple = adults == 2;
        var withKids = size - adults > 0;

        var built = new List<NewMember>(seats.Count);
        foreach (var seat in seats)
            built.Add(BuildMember(household, seat, built, couple, withKids));

        if (couple && built.Count > 1)
            PointHeadAtSpouse(built[0], built[1].Person);

        return built;
    }

    // The head is built before the spouse exists, so a couple's head gets the
    // spouse as next of kin afterwards.
    private static void PointHeadAtSpouse(NewMember head, Person spouse)
    {
        var name = $"{spouse.FirstName} {spouse.LastName}";
        head.Member.NextOfKin = name;
        head.Member.EmergencyContactName = name;
        head.Member.EmergencyContactPhoneNumber = spouse.PhoneNumber;
    }

    private Household NewHousehold(DateOnly joinedOn, double commitment, string lastName)
    {
        var leftOn = DecideDeparture(joinedOn, commitment);
        var status = leftOn is null ? MemberStatus.Active : rng.Weighted(_departureStatuses);

        return new Household(
            rng.NewGuid(),
            joinedOn,
            commitment,
            GivingFactory.PickBand(rng),
            leftOn,
            status,
            NewResidence(),
            lastName
        );
    }

    private int AdultsIn(int size) =>
        size switch
        {
            1 => 1,
            2 => rng.Chance(0.7) ? 2 : 1,
            _ => rng.Chance(0.84) ? 2 : 1,
        };

    private List<Seat> PlanSeats(Household h, int adults, int children)
    {
        var withKids = children > 0;
        var seats = new List<Seat>(adults + children);

        var headGender =
            adults == 2 ? (rng.Chance(0.7) ? Gender.Male : Gender.Female)
            : rng.Chance(withKids ? 0.82 : 0.58) ? Gender.Female
            : Gender.Male;
        var headAge = Math.Clamp(
            rng.Gaussian(withKids ? 36 : 31, withKids ? 7 : 10),
            withKids ? 24 : 18,
            72
        );
        var headDob = DobFor(h.JoinedOn, headAge);
        seats.Add(new Seat(Role.Head, headGender, headDob, h.JoinedOn));

        var youngestParent = headDob;

        if (adults == 2)
        {
            var spouseGender = headGender == Gender.Male ? Gender.Female : Gender.Male;
            var gap = headGender == Gender.Male ? -3.0 : 3.0;
            var spouseAge = Math.Max(18.0, headAge + rng.Gaussian(gap, 3.5));
            var spouseDob = DobFor(h.JoinedOn, spouseAge);

            var spouseJoined = rng.Chance(0.85)
                ? h.JoinedOn
                : Earliest(h.JoinedOn.AddDays(rng.Next(7, 700)), _today);
            if (h.LeftOn is { } left && spouseJoined >= left)
                spouseJoined = h.JoinedOn;

            seats.Add(new Seat(Role.Spouse, spouseGender, spouseDob, spouseJoined));
            youngestParent = spouseDob > headDob ? spouseDob : headDob;
        }

        AddChildren(seats, h, children, youngestParent);
        return seats;
    }

    private void AddChildren(List<Seat> seats, Household h, int children, DateOnly youngestParent)
    {
        var tenureYears = (_today.DayNumber - h.JoinedOn.DayNumber) / 365.25;
        var youngestBirth = youngestParent.AddYears(19);
        var latestBirth = (h.LeftOn ?? _today).AddDays(-30);

        // A household that cannot plausibly have had children yet has none.
        if (youngestBirth > latestBirth)
            return;

        for (var i = 0; i < children; i++)
        {
            // Negative ages are children born after the household joined.
            var ageAtJoin = rng.Range(-Math.Min(tenureYears, 8.0), 17.0);
            var dob = DobFor(h.JoinedOn, ageAtJoin);
            dob = dob < youngestBirth ? youngestBirth.AddDays(rng.Next(0, 400)) : dob;
            dob = dob > latestBirth ? latestBirth : dob;

            var joined = dob >= h.JoinedOn ? dob.AddDays(rng.Next(14, 120)) : h.JoinedOn;
            joined = Earliest(joined, h.LeftOn?.AddDays(-1) ?? _today);

            seats.Add(
                new Seat(Role.Child, rng.Chance(0.5) ? Gender.Male : Gender.Female, dob, joined)
            );
        }
    }

    // Most people who leave do it in their first years, so departures skew early.
    private DateOnly? DecideDeparture(DateOnly joinedOn, double commitment)
    {
        var tenureDays = _today.DayNumber - joinedOn.DayNumber;
        var tenureYears = tenureDays / 365.25;
        if (tenureYears < 0.4)
            return null;

        var hazard = 0.03 * (1.6 - commitment * 1.2);
        var chanceOfLeaving = 1.0 - Math.Exp(-hazard * tenureYears);
        if (!rng.Chance(chanceOfLeaving))
            return null;

        var offset = (int)(tenureDays * Math.Pow(rng.NextDouble(), 1.4));
        return joinedOn.AddDays(Math.Clamp(offset, 90, tenureDays - 14));
    }

    // ── One member ────────────────────────────────────────────────────────

    private NewMember BuildMember(
        Household h,
        Seat seat,
        IReadOnlyList<NewMember> household,
        bool couple,
        bool withKids
    )
    {
        var head = household.Count > 0 ? household[0].Person : null;
        var age = AgeOn(seat.DateOfBirth, _today);

        var firstName = FirstNameFor(seat.Gender);
        var lastName =
            seat is { Role: Role.Spouse } && rng.Chance(0.1) ? faker.Name.LastName() : h.LastName;

        var person = new Person
        {
            Id = rng.NewGuid(),
            CongregationId = congregation.Id,
            Kind = PersonKind.Member,
            FirstName = firstName,
            LastName = lastName,
            OtherNames = rng.Chance(age >= 18 ? 0.25 : 0.1) ? FirstNameFor(seat.Gender) : null,
            PhoneNumber = age < 16 && head is not null ? head.PhoneNumber : NewPhone(),
            EmailAddress = age >= 18 && rng.Chance(0.55) ? NewEmail(firstName, lastName) : null,
            CreatedAt = Stamp(seat.JoinedOn),
        };

        var member = NewMemberRow(person, h, seat, head, couple, withKids);
        var profile = NewProfile(person.Id, h, seat, age);
        return new NewMember(person, member, profile);
    }

    private Member NewMemberRow(
        Person person,
        Household h,
        Seat seat,
        Person? head,
        bool couple,
        bool withKids
    )
    {
        var age = AgeOn(seat.DateOfBirth, _today);

        // A spouse or child lists the head as next of kin and emergency contact.
        // The head lists a relative, or the spouse once the spouse exists.
        var close = seat.Role == Role.Head ? null : head;
        var kin = close is null
            ? RandomRelative(seat.Gender, h.LastName)
            : $"{close.FirstName} {close.LastName}";

        var knownHometown = seat.Role == Role.Child || rng.Chance(0.5);

        return new Member
        {
            PersonId = person.Id,
            CongregationId = congregation.Id,
            DateOfBirth = seat.DateOfBirth,
            JoinedDate = JoinedDateFor(seat.JoinedOn),
            Gender = seat.Gender,
            ResidentialAddress = h.Residence.Address,
            City = h.Residence.City,
            Hometown = knownHometown ? h.Residence.Hometown : faker.Address.City(),
            Region = h.Residence.Region,
            GpsAddress = h.Residence.GpsAddress,
            MaritalStatus = MaritalStatusFor(seat.Role, age, couple, withKids),
            NextOfKin = kin,
            EmergencyContactName = kin,
            EmergencyContactPhoneNumber = close?.PhoneNumber ?? NewPhone(),
            Status = h.Status,
            CreatedAt = person.CreatedAt,
        };
    }

    private PersonProfile NewProfile(Guid personId, Household h, Seat seat, int age)
    {
        var commitment = age switch
        {
            < 13 => h.Commitment,
            < 20 => h.Commitment * 0.8,
            _ => Math.Clamp(h.Commitment + rng.Gaussian(0, 0.08), 0.02, 1.0),
        };

        return new PersonProfile
        {
            Id = personId,
            HouseholdId = h.Id,
            Gender = seat.Gender,
            DateOfBirth = seat.DateOfBirth,
            JoinedOn = seat.JoinedOn,
            LeftOn = h.LeftOn,
            Status = h.Status,
            Commitment = commitment,
            Giving = GivingFactory.Create(rng, age, commitment, h.Income),
        };
    }

    // A few old records never had a join date written down.
    private DateOnly? JoinedDateFor(DateOnly joinedOn) =>
        joinedOn < _today.AddYears(-3) && rng.Chance(0.04) ? null : joinedOn;

    private MaritalStatus MaritalStatusFor(Role role, int age, bool couple, bool withKids)
    {
        if (role == Role.Child)
            return MaritalStatus.Single;
        if (couple)
            return MaritalStatus.Married;

        return age switch
        {
            < 30 => rng.Weighted<MaritalStatus>([
                (MaritalStatus.Single, 95.0),
                (MaritalStatus.Married, 5.0),
            ]),
            < 60 => rng.Weighted<MaritalStatus>([
                (MaritalStatus.Single, withKids ? 45.0 : 75.0),
                (MaritalStatus.Married, 15.0),
                (MaritalStatus.Widowed, withKids ? 30.0 : 8.0),
            ]),
            _ => rng.Weighted<MaritalStatus>([
                (MaritalStatus.Widowed, 35.0),
                (MaritalStatus.Married, 30.0),
                (MaritalStatus.Single, 35.0),
            ]),
        };
    }

    // ── Visitors ──────────────────────────────────────────────────────────

    public NewVisitor BuildVisitor()
    {
        var gender = rng.Chance(0.55) ? Gender.Female : Gender.Male;
        var firstVisit = SampleFirstVisit();
        var convertedOn = SampleConversion(firstVisit);
        var firstName = FirstNameFor(gender);
        var lastName = faker.Name.LastName();

        var person = new Person
        {
            Id = rng.NewGuid(),
            CongregationId = congregation.Id,
            Kind = convertedOn is null ? PersonKind.Visitor : PersonKind.Member,
            FirstName = firstName,
            LastName = lastName,
            OtherNames = null,
            PhoneNumber = NewPhone(),
            EmailAddress = rng.Chance(0.4) ? NewEmail(firstName, lastName) : null,
            CreatedAt = Stamp(firstVisit),
        };

        var visitor = new Visitor
        {
            PersonId = person.Id,
            CongregationId = congregation.Id,
            Notes = rng.Chance(0.4) ? rng.Pick(_visitorNotes) : null,
            ConvertedAt = convertedOn is null ? null : Stamp(convertedOn.Value),
            CreatedAt = person.CreatedAt,
        };

        var story = new VisitorProfile(person.Id, firstVisit, convertedOn);
        if (convertedOn is not { } joinedOn)
            return new NewVisitor(person, visitor, null, null, story);

        // A converted visitor becomes a member of a new household of their own.
        var age = Math.Clamp(rng.Gaussian(31, 9), 18, 65);
        var household = NewHousehold(
            joinedOn,
            Math.Clamp(rng.Gaussian(0.62, 0.2), 0.1, 0.99),
            lastName
        );
        var seat = new Seat(Role.Head, gender, DobFor(joinedOn, age), joinedOn);

        var member = NewMemberRow(
            person,
            household,
            seat,
            head: null,
            couple: false,
            withKids: false
        );
        member.CreatedAt = visitor.ConvertedAt!.Value;
        var profile = NewProfile(person.Id, household, seat, AgeOn(seat.DateOfBirth, _today));

        return new NewVisitor(person, visitor, member, profile, story);
    }

    // Most visitors are recent: the further back, the fewer are still on file.
    private DateOnly SampleFirstVisit()
    {
        var maxDays = Math.Max(30, _today.DayNumber - growth.Founded.DayNumber);
        var days = (int)Math.Min(rng.Exponential(300), maxDays);
        var date = _today.AddDays(-days);

        if (!rng.Chance(0.8))
            return date;

        var sunday = date.AddDays(-(int)date.DayOfWeek);
        return sunday >= growth.Founded ? sunday : date;
    }

    // About one visitor in seven becomes a member, usually after a couple of
    // months. Someone who visited last week cannot have converted yet.
    private DateOnly? SampleConversion(DateOnly firstVisit)
    {
        if (!rng.Chance(0.14))
            return null;

        var days = (int)Math.Max(14, rng.LogNormal(75, 0.7));
        var on = firstVisit.AddDays(days);
        if (on > _today)
            return null;

        var sunday = on.AddDays(-(int)on.DayOfWeek);
        return sunday >= firstVisit.AddDays(7) ? sunday : on;
    }

    // ── Names, contact details, places ────────────────────────────────────

    private string FirstNameFor(Gender gender) =>
        faker.Name.FirstName(gender == Gender.Male ? BogusGender.Male : BogusGender.Female);

    private string RandomRelative(Gender gender, string lastName) =>
        $"{FirstNameFor(rng.Chance(0.5) ? gender : (gender == Gender.Male ? Gender.Female : Gender.Male))} {lastName}";

    private string NewPhone() =>
        $"{rng.Pick(_mobilePrefixes)}-{rng.Next(100, 1000)}-{rng.Next(1000, 10000)}";

    private string NewEmail(string firstName, string lastName) =>
        faker.Internet.Email(firstName, lastName, rng.Weighted(_mailProviders));

    private Residence NewResidence()
    {
        var city = rng.Weighted<string>([
            (congregation.City, 60.0),
            (congregation.Town, 25.0),
            (faker.Address.City(), 15.0),
        ]);

        var region = rng.Chance(0.65) ? congregation.Region : rng.Weighted(_regions);
        var hometown = rng.Chance(0.45) ? city : faker.Address.City();
        var gps = rng.Chance(0.65)
            ? $"{RegionCode(region)}-{rng.Next(100, 1000)}-{rng.Next(1000, 10000)}"
            : null;

        return new Residence(faker.Address.StreetAddress(), city, hometown, region, gps);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    // A timestamp on that day, during the morning or afternoon, never in the future.
    private DateTime Stamp(DateOnly date)
    {
        var at = date.ToDateTime(new TimeOnly(rng.Next(8, 16), rng.Next(60)), DateTimeKind.Utc);
        return at > now ? now : at;
    }

    private static DateOnly DobFor(DateOnly on, double ageYears) =>
        on.AddDays(-(int)(ageYears * 365.25));

    private static int AgeOn(DateOnly dob, DateOnly date)
    {
        var age = date.Year - dob.Year;
        return date < dob.AddYears(age) ? age - 1 : age;
    }

    private static DateOnly Earliest(DateOnly a, DateOnly b) => a < b ? a : b;

    private static string RegionCode(Region region) =>
        region switch
        {
            Region.GreaterAccra => "GA",
            Region.Ashanti => "AK",
            Region.Eastern => "EN",
            Region.Central => "CR",
            Region.Western => "WR",
            Region.WesternNorth => "WN",
            Region.Volta => "VR",
            Region.Oti => "OT",
            Region.Northern => "NR",
            Region.Savannah => "SV",
            Region.NorthEast => "NE",
            Region.UpperEast => "UE",
            Region.UpperWest => "UW",
            Region.Bono => "BA",
            Region.BonoEast => "BE",
            _ => "AF",
        };

    // ── Reference data ────────────────────────────────────────────────────

    private static readonly IReadOnlyList<(int Value, double Weight)> _householdSizes =
    [
        (1, 24.0),
        (2, 22.0),
        (3, 20.0),
        (4, 19.0),
        (5, 10.0),
        (6, 5.0),
    ];

    private static readonly IReadOnlyList<(MemberStatus Value, double Weight)> _departureStatuses =
    [
        (MemberStatus.Inactive, 60.0),
        (MemberStatus.Transferred, 22.0),
        (MemberStatus.Archived, 18.0),
    ];

    private static readonly IReadOnlyList<(Region Value, double Weight)> _regions =
    [
        (Region.GreaterAccra, 25.0),
        (Region.Ashanti, 20.0),
        (Region.Eastern, 8.0),
        (Region.Central, 8.0),
        (Region.Western, 7.0),
        (Region.Northern, 6.0),
        (Region.Volta, 6.0),
        (Region.Bono, 4.0),
        (Region.BonoEast, 3.0),
        (Region.WesternNorth, 3.0),
        (Region.UpperEast, 3.0),
        (Region.UpperWest, 2.5),
        (Region.Ahafo, 2.0),
        (Region.Oti, 1.5),
        (Region.Savannah, 1.5),
        (Region.NorthEast, 1.5),
    ];

    private static readonly IReadOnlyList<string> _mobilePrefixes =
    [
        "024",
        "054",
        "055",
        "059",
        "020",
        "050",
        "027",
        "057",
        "026",
        "056",
    ];

    private static readonly IReadOnlyList<(string Value, double Weight)> _mailProviders =
    [
        ("gmail.com", 70.0),
        ("yahoo.com", 15.0),
        ("outlook.com", 10.0),
        ("icloud.com", 5.0),
    ];

    private static readonly IReadOnlyList<string> _visitorNotes =
    [
        "Invited by a member.",
        "Moved into the area recently.",
        "Visiting from another church.",
        "Came for a special service.",
        "Brought by a family member.",
        "Found the church online.",
        "Asked about the next membership class.",
    ];
}
