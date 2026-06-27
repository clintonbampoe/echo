namespace Api.Core.Dtos.Interfaces;

public interface IReferenceCreateDto
{
    string Name { get; }
}

public interface IReferenceUpdateDto
{
    string Name { get; }
}

public interface IReferenceResponseDto
{
    int Id { get; }
    string Name { get; }
}
