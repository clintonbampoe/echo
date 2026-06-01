namespace Backend.Api.Core.Services.Interfaces;

public interface ICreateNewRecordService<T>
{
    Task<bool> CreateNewRecord(T newRecordData, CancellationToken cancellationToken = default);
}
