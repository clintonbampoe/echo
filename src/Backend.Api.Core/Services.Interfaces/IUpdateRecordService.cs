namespace Backend.Api.Core.Services.Interfaces;

public interface IUpdateRecordService<T>
{
    Task<bool> UpdateRecord(
        Guid Id, T updatedRecordData, CancellationToken cancellationToken = default);
}
