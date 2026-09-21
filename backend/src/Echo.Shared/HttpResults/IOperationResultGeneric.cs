namespace Echo.Shared.HttpResults;

public interface IOperationResult<T> : IOperationResult
{
    T? Data { get; }
}
