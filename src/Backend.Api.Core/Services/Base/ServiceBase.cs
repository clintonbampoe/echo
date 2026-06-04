using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Base;

namespace Backend.Api.Core.Services.Base;

public abstract class ServiceBase<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    protected readonly EntityRepositoryBase<T> _repository;
    protected readonly IMapper _mapper;
    public ServiceBase(EntityRepositoryBase<T> entityRepositoryBase, IMapper mapper)
    {
        _repository = entityRepositoryBase;
        _mapper = mapper;
    }

    public virtual async Task<IOperationResult> GetPagedAsync(
            PaginationParameters paginationParameters,
            QueryParameters queryParameters,
            CancellationToken ct
        )
    {
        var pagedEntities = await _repository.GetPageAsync(paginationParameters, queryParameters, ct);
        var mappedRecords = _mapper.Map<List<IListResponseDto<T>>>(pagedEntities.Data);

        var pagedResponse = new PagedResponse<IListResponseDto<T>>(
            mappedRecords,
            paginationParameters,
            pagedEntities.TotalRecordCount
        );

        return new SuccessResult<PagedResponse<IListResponseDto<T>>>(pagedResponse);
    }

    public virtual async Task<IOperationResult> GetByIdAsync(
            Guid id,
            CancellationToken ct
        )
    {
        var entity = await _repository.GetByIdAsync(id, ct);

        if (entity == null)
            return new NotFoundResult($"Record not found.");

        var mappedDto = _mapper.Map<IResponseDto<T>>(entity);
        return new SuccessResult<IResponseDto<T>>(mappedDto);
    }

    public virtual async Task<IOperationResult> CreateNewRecord(
            ICreateDto<T> createRecordDto,
            CancellationToken ct
        )
    {
        var entity = _mapper.Map<T>(createRecordDto);

        var recordCreatedSuccessfully = await _repository.CreateRecord(entity, ct);

        if (!recordCreatedSuccessfully)
            return new BadRequestResult("Operation failed.");

        return new OkResult("Record created successfully.");

    }

    public virtual async Task<IOperationResult> UpdateRecord(
            Guid id,
            IUpdateDto<T> updateRecordDto,
            CancellationToken ct
       )
    {
        var existingEntity = await _repository.GetByIdAsync(id);

        if (existingEntity is null)
            return new NotFoundResult("Record not found.");

        var entity = _mapper.Map<T>(updateRecordDto);

        var recordUpdateSuccessfully = await _repository.UpdateRecord(id, entity, ct);

        if (!recordUpdateSuccessfully)
            return new BadRequestResult("Operation failed.");

        return new OkResult("Record updated successfully.");
    }

    public virtual async Task<IOperationResult> DeleteRecord(
            Guid id,
            CancellationToken ct
        )
    {
        var success = await _repository.DeleteRecord(id, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        return new OkResult("Record deleted successfully.");
    }
}
