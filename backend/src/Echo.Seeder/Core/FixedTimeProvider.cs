namespace Echo.Seeder.Core;

/// <summary>
/// A TimeProvider that always returns the same instant.
///
/// Every "now" in the seeder resolves to SeederOptions.ReferenceNow, making
/// all generated timestamps identical across machines and runs for the same seed.
/// </summary>
public sealed class FixedTimeProvider(DateTimeOffset instant) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => instant;
}
