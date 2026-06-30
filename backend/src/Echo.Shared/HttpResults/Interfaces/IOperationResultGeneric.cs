namespace Echo.Shared.HttpResults.Interfaces;

public interface IOperationResult<T> : IOperationResult
{
    T? Data { get; }
}
