namespace Backend.Api.Core.Services.Interfaces;

public interface IUpdateEntityEngine<T>
{
    Task<bool> UpdateEntityById(
        Guid Id, T updatedRecordData, CancellationToken cancellationToken = default);
}
