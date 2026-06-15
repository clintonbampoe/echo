namespace Backend.Api.Core.Dtos.Interfaces;

public interface IPrimaryCreateDto<T> { }

public interface IPrimaryUpdateDto<T> { }

public interface IPrimaryListResponseDto<T>
{
    Guid Id { get; }
}

public interface IPrimaryResponseDto<T>
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
}
