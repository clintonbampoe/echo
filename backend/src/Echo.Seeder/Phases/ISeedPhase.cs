using Echo.Seeder.Core;

namespace Echo.Seeder.Phases;

/// <summary>
/// One step in the seed pipeline.
///
/// Phases run in a fixed sequence. Each phase reads from ctx.State (populated
/// by earlier phases) and writes its own output back into it. The Name is used
/// </summary>
public interface ISeedPhase
{
    string Name { get; }
    Task RunAsync(SeederContext ctx, CancellationToken ct);
}
