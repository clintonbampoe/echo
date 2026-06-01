namespace Backend.Api.Core.Services.Interfaces;

public interface IUpdateRecordService<T>
{
    Task<bool> UpdateRecordById(
        Guid Id, T updatedRecordData, CancellationToken cancellationToken = default);
}
