namespace Backend.Api.Core.Dtos.Interfaces;

public interface IReferenceCreateDto<T>
{
    string Name { get; init; }
}

public interface IReferenceUpdateDto<T>
{
    string Name { get; init; }
}

public interface IReferenceResponseDto<T>
{
    int Id { get; }
    string Name { get; }
}
