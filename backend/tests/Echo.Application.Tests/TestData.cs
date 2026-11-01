using AutoFixture;

namespace Echo.Application.Tests;

/// <summary>
/// Shared, deterministic test data generation.
/// </summary>
/// <remarks>
/// AutoFixture has no seed API (upstream issue #670, closed won't-fix) and its defaults are random,
/// which would make Verify snapshots differ on every run. Every generator below is therefore a
/// deterministic sequence, which keeps snapshot content stable across runs.
/// <para>
/// <see cref="Guid"/> and <see cref="DateTime"/> need no registration: Verify already renders them as
/// scrubbed tokens (<c>Guid_1</c>, <c>DateTime_1</c>).
/// </para>
/// <para>
/// Properties carrying a <c>[Range]</c> attribute are the exception: AutoFixture honours the
/// attribute through an engine-level node that fixture customizations cannot out-prioritise, and
/// AutoFixture 4.x exposes no way to seed its randomness. Pin such fields explicitly in the test
/// with <c>.With(...)</c>, or the snapshot will differ between runs.
/// </para>
/// <para>
/// The domain navigation graph is acyclic, so recursion handling is unnecessary. If a cycle is ever
/// introduced, this becomes <c>OmitOnRecursionBehavior</c>.
/// </para>
/// </remarks>
public static class TestData
{
    /// <summary>
    /// A shared fixture for tests that need a plausible graph but do not assert on exact values.
    /// Prefer <see cref="NewFixture"/> in mapper tests, where snapshot content depends on per-test
    /// generator state.
    /// </summary>
    public static Fixture Shared { get; } = NewFixture();

    /// <summary>A stable id for tests that only need "some other id".</summary>
    public static readonly Guid DefaultId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>A fixture producing deterministic values suitable for snapshot assertions.</summary>
    public static Fixture NewFixture()
    {
        var fixture = new Fixture();

        // AutoFixture defaults strings to random GUIDs; Verify does not scrub those, so snapshots
        // would change every run. A per-fixture counter is stable because AutoFixture walks
        // properties in a fixed order.
        var strings = 0;
        fixture.Register(() => $"v{++strings}");

        // Verify does not scrub numbers. Nullable forms need their own registration: AutoFixture
        // does not fall back to the underlying type's generator.
        var ints = 0;
        fixture.Register(() => ++ints);
        fixture.Register<int?>(() => ++ints);

        var decimals = 0m;
        fixture.Register(() => ++decimals);
        fixture.Register<decimal?>(() => ++decimals);

        // AutoFixture 4.x has no DateOnly/TimeOnly generator: DateOnly throws
        // ObjectCreationExceptionWithPath and TimeOnly yields an unrepresentable value.
        var date = new DateOnly(2026, 1, 1);
        var dateIndex = 0;
        fixture.Register(() => date.AddDays(++dateIndex));

        var time = new TimeOnly(9, 0);
        var timeIndex = 0;
        fixture.Register(() => time.AddMinutes(++timeIndex));

        return fixture;
    }

    /// <summary>
    /// Builds <paramref name="count"/> values from one fixture.
    /// </summary>
    /// <remarks>
    /// Sharing the fixture is what makes the values distinct within a single snapshot: a fresh
    /// fixture per value would restart every generator and produce identical rows.
    /// </remarks>
    public static List<T> Many<T>(int count, Func<T> create) =>
        [.. Enumerable.Range(0, count).Select(_ => create())];

    /// <summary>
    /// Builds a value from <paramref name="fixture"/> while holding its lock.
    /// </summary>
    /// <remarks>
    /// <see cref="Fixture"/> is not thread safe, so the process-wide <see cref="Shared"/> instance
    /// must not be used directly from parallel test classes. Going through here keeps it safe while
    /// per-test fixtures stay fully parallel. The lock is re-entrant, which is what lets
    /// <c>CreateEvent</c> call <c>CreateOrganization</c> without deadlocking.
    /// </remarks>
    public static T Build<T>(Fixture fixture, Func<T> build)
    {
        lock (fixture)
            return build();
    }
}
