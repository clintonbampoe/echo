namespace Backend.Api.Core.Dtos.Interfaces;

public interface IPrimaryCreateDto { }

public interface IPrimaryUpdateDto { }

public interface IPrimaryListResponseDto
{
    Guid Id { get; }
}

public interface IPrimaryResponseDto
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
}
