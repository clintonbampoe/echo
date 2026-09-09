namespace Echo.Application.Services.Generators;

public class IdGenerator() : IIdGenerator
{
    public Guid Generate() => Guid.CreateVersion7();
}
