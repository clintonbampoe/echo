namespace Echo.Application.HttpResults;

public interface IOperationResult<T> : IOperationResult
{
    T? Data { get; }
}
