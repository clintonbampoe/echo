using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Services.Interfaces;

namespace Backend.Api.Core.Services;

public class DomainRecordService<T> : IDomainRecordService<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    private readonly IGetRecordService<T> _getRecordService;
    private readonly ICreateNewRecordService<T> _createNewRecordService;
    private readonly IUpdateRecordService<T> _updateRecordService;
    private readonly ISoftDeleteService<T> _softDeleteService;

    public DomainRecordService(
        IGetRecordService<T> getRecordService,
        ICreateNewRecordService<T> createNewRecordService,
        IUpdateRecordService<T> updateRecordService,
        ISoftDeleteService<T> softDeleteService
        )
    {
        _getRecordService = getRecordService;
        _createNewRecordService = createNewRecordService;
        _updateRecordService = updateRecordService;
        _softDeleteService = softDeleteService;
    }

    public Task<T?> GetRecordByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return _getRecordService.GetRecordByIdAsync(Id, cancellationToken);
    }

    public int GetTotalRecordsCount(QueryParameters queryParameters)
    {
        return _getRecordService.GetTotalRecordsCount(queryParameters);
    }

    public PagedResponse<T> CreateNewPagedResponseObject(List<T> records, PaginationParams paginationParameters, int totalRecords)
    {
        return _getRecordService.CreateNewPagedResponseObject(records, paginationParameters, totalRecords);
    }

    public Task<bool> CreateNewRecord(T newRecordData, CancellationToken cancellationToken = default)
    {
        return _createNewRecordService.CreateNewRecord(newRecordData, cancellationToken);
    }

    public Task<bool> UpdateRecordById(Guid id, T updatedRecordData, CancellationToken cancellationToken = default)
    {
        return _updateRecordService.UpdateRecordById(id, updatedRecordData, cancellationToken);
    }

    public Task<bool> SoftDeleteByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return _softDeleteService.SoftDeleteByIdAsync(Id, cancellationToken);
    }
}
