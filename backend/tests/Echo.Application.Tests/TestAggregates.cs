using AutoFixture;
using Echo.Domain;
using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Domain.Events;
using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Domain.Projects;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Echo.Domain.Users;

namespace Echo.Application.Tests;

/// <summary>
/// Aggregate builders for <see cref="Fixture"/>.
/// </summary>
/// <remarks>
/// These populate navigation properties, because mappers flatten through them and a null
/// navigation would silently produce an empty string instead of exercising the mapping. Everything
/// else is left to AutoFixture.
/// <para>
/// Mapper tests should prefer <c>fixture.Create&lt;T&gt;()</c> with explicit <c>.With(...)</c> so the
/// snapshot shows meaningful values; these helpers exist for the query, window and ordering tests
/// that only need a plausible graph.
/// </para>
/// </remarks>
public static class TestAggregates
{
    /// <summary>
    /// Clears <see cref="ISoftDeletable.DeletedAt"/>.
    /// </summary>
    /// <remarks>
    /// AutoFixture populates every writable property, so <c>DeletedAt</c> comes back as a non-null
    /// timestamp. A newly constructed aggregate is not soft-deleted, and query helpers such as
    /// <c>Live()</c> filter it out. Tests that want a deleted row should set it explicitly.
    /// </remarks>
    public static T Live<T>(this T entity)
        where T : ISoftDeletable
    {
        entity.DeletedAt = null;
        return entity;
    }

    // Leaves - no navigation properties.

    public static Congregation CreateCongregation(this Fixture fixture) =>
        TestData.Build(fixture, () => fixture.Create<Congregation>().Live());

    public static User CreateUser(this Fixture fixture) =>
        TestData.Build(fixture, () => fixture.Create<User>().Live());

    public static Person CreatePerson(this Fixture fixture) =>
        TestData.Build(fixture, () => fixture.Create<Person>().Live());

    public static SessionLabel CreateSessionLabel(this Fixture fixture) =>
        TestData.Build(fixture, () => fixture.Create<SessionLabel>().Live());

    public static AssetCategory CreateAssetCategory(this Fixture fixture) =>
        TestData.Build(fixture, () => fixture.Create<AssetCategory>().Live());

    public static ProjectCategory CreateProjectCategory(this Fixture fixture) =>
        TestData.Build(fixture, () => fixture.Create<ProjectCategory>().Live());

    public static TransactionCategory CreateTransactionCategory(this Fixture fixture) =>
        TestData.Build(fixture, () => fixture.Create<TransactionCategory>().Live());

    public static Organization CreateOrganization(this Fixture fixture) =>
        TestData.Build(fixture, () => fixture.Create<Organization>().Live());

    // Aggregates with a single navigation property.

    public static Member CreateMember(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture.Build<Member>().With(m => m.Person, fixture.CreatePerson()).Create().Live()
        );

    public static Visitor CreateVisitor(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture.Build<Visitor>().With(v => v.Person, fixture.CreatePerson()).Create().Live()
        );

    public static Session CreateSession(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<Session>()
                    .With(s => s.SessionLabel, fixture.CreateSessionLabel())
                    .With(s => s.Creator, fixture.CreateUser())
                    .Create()
                    .Live()
        );

    public static Asset CreateAsset(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<Asset>()
                    .With(a => a.Category, fixture.CreateAssetCategory())
                    .Create()
                    .Live()
        );

    public static Project CreateProject(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<Project>()
                    .With(p => p.Category, fixture.CreateProjectCategory())
                    .With(p => p.Manager, fixture.CreateMember())
                    .Create()
                    .Live()
        );

    public static Transaction CreateTransaction(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<Transaction>()
                    .With(t => t.Category, fixture.CreateTransactionCategory())
                    .Create()
                    .Live()
        );

    public static Tithe CreateTithe(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () => fixture.Build<Tithe>().With(t => t.Member, fixture.CreateMember()).Create().Live()
        );

    public static Attendance CreateAttendance(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<Attendance>()
                    .With(a => a.Session, fixture.CreateSession())
                    .With(a => a.Person, fixture.CreatePerson())
                    .Create()
                    .Live()
        );

    public static EventRegistration CreateEventRegistration(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<EventRegistration>()
                    .With(r => r.Member, fixture.CreateMember())
                    .With(r => r.Event, fixture.CreateEvent())
                    .Create()
                    .Live()
        );

    public static EventAttendance CreateEventAttendance(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<EventAttendance>()
                    .With(a => a.Member, fixture.CreateMember())
                    .With(a => a.Event, fixture.CreateEvent())
                    .Create()
                    .Live()
        );

    public static ProjectContribution CreateProjectContribution(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<ProjectContribution>()
                    .With(c => c.Project, fixture.CreateProject())
                    .Create()
                    .Live()
        );

    public static OrganizationMember CreateOrganizationMember(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<OrganizationMember>()
                    .With(m => m.Member, fixture.CreateMember())
                    .With(m => m.Organization, fixture.CreateOrganization())
                    .Create()
                    .Live()
        );

    public static Event CreateEvent(this Fixture fixture) =>
        TestData.Build(
            fixture,
            () =>
                fixture
                    .Build<Event>()
                    .With(e => e.Organization, fixture.CreateOrganization())
                    .With(e => e.Organizer, fixture.CreateMember())
                    .Create()
                    .Live()
        );
}
