namespace Echo.Shared.Services.Generators;

public class IdGenerator() : IIdGenerator
{
    public Guid Generate() => Guid.CreateVersion7();
}
