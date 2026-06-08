using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Base;

namespace Backend.Api.Core.Services.Base;

public abstract class ServiceBase<T>
    where T : class, ICongregationEntity, ISoftDeletableEntity
{
    protected virtual RepositoryBase<T> Repository { get; }
    protected readonly IMapper _mapper;

    public ServiceBase(RepositoryBase<T> repository, IMapper mapper)
    {
        Repository = repository;
        _mapper = mapper;
    }

    public virtual async Task<IOperationResult> GetPagedAsync<TListResponseDto>(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
        where TListResponseDto : class, IListResponseDto<T>
    {
        var pagedEntities = await Repository.GetPageAsync(
            paginationParameters,
            queryParameters,
            ct
        );
        var mappedRecords = _mapper.Map<List<TListResponseDto>>(pagedEntities.Data);

        var pagedResponse = new PagedResponse<TListResponseDto>(
            mappedRecords,
            paginationParameters,
            pagedEntities.TotalRecordCount
        );

        return new SuccessResult<PagedResponse<TListResponseDto>>(pagedResponse);
    }

    public virtual async Task<IOperationResult> GetByIdAsync<TResponseDto>(
        Guid id,
        CancellationToken ct
    )
        where TResponseDto : IResponseDto<T>
    {
        var entity = await Repository.GetByIdAsync(id, ct);

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

        var recordCreatedSuccessfully = await Repository.CreateRecord(entity, ct);

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
        var existingEntity = await Repository.GetByIdAsync(id);

        if (existingEntity is null)
            return new NotFoundResult("Record not found.");

        var entity = _mapper.Map<T>(updateRecordDto);

        var recordUpdateSuccessfully = await Repository.UpdateRecord(id, entity, ct);

        if (!recordUpdateSuccessfully)
            return new BadRequestResult("Operation failed.");

        return new OkResult("Record updated successfully.");
    }

    public virtual async Task<IOperationResult> DeleteRecord(Guid id, CancellationToken ct)
    {
        var success = await Repository.DeleteRecord(id, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        return new OkResult("Record deleted successfully.");
    }
}
