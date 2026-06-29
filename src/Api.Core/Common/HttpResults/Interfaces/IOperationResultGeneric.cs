namespace Api.Core.Common.HttpResults.Interfaces;

public interface IOperationResult<T> : IOperationResult
{
    T? Data { get; }
}
