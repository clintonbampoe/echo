using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Engines.Interfaces;

namespace Backend.Api.Core.Repositories.Engines;

public class DatabaseEngine<T> : IDatabaseEngine<T>
    where T : ICongregationEntity, ISoftDeletableEntity
{
    private readonly IGetEntityEngine<T> _getRecordService;
    private readonly ICreateEntityEngine<T> _createNewRecordService;
    private readonly IUpdateEntityEngine<T> _updateRecordService;
    private readonly ISoftDeleteEntityEngine<T> _softDeleteService;

    public DatabaseEngine(
        IGetEntityEngine<T> getRecordService,
        ICreateEntityEngine<T> createNewRecordService,
        IUpdateEntityEngine<T> updateRecordService,
        ISoftDeleteEntityEngine<T> softDeleteService
    )
    {
        _getRecordService = getRecordService;
        _createNewRecordService = createNewRecordService;
        _updateRecordService = updateRecordService;
        _softDeleteService = softDeleteService;
    }

    public Task<T?> GetEntityByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return _getRecordService.GetEntityByIdAsync(Id, cancellationToken);
    }

    public PagedResponse<T> CreateNewPagedResponseObject(
        List<T> records,
        PaginationParameters paginationParameters,
        int totalRecords
    )
    {
        return _getRecordService.CreateNewPagedResponseObject(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public Task<bool> CreateNewEntity(
        T newRecordData,
        CancellationToken cancellationToken = default
    )
    {
        return _createNewRecordService.CreateNewEntity(newRecordData, cancellationToken);
    }

    public Task<bool> UpdateEntityById(
        Guid id,
        T updatedRecordData,
        CancellationToken cancellationToken = default
    )
    {
        return _updateRecordService.UpdateEntityById(id, updatedRecordData, cancellationToken);
    }

    public Task<bool> SoftDeleteByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return _softDeleteService.SoftDeleteByIdAsync(Id, cancellationToken);
    }

    public Task<PagedResponse<T>> GetPagedEntityListByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }
}
