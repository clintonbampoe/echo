using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos.Interfaces;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core.Interfaces;

namespace Echo.Core.Services.Base;

public abstract class PrimaryServiceBase<T, TResponseDto>(
    PrimaryRepositoryBase<T> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
)
    where T : class, IPrimaryEntity
    where TResponseDto : IPrimaryResponseDto
{
    protected readonly PrimaryRepositoryBase<T> Repository = repository;
    protected readonly IUnitOfWork UnitOfWork = unitOfWork;
    protected readonly IMapper Mapper = mapper;

    public abstract Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    );

    public abstract Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    );

    public virtual async Task<IOperationResult> CreateAsync(
        Guid congregationId,
        IPrimaryCreateDto dto,
        CancellationToken ct = default
    )
    {
        var entity = Mapper.Map<T>(dto);
        entity.CongregationId = congregationId;

        await Repository.CreateRecord(entity, ct);
        await UnitOfWork.CommitAsync(ct);

        var resource = Mapper.Map<TResponseDto>(entity);
        return new CreatedAtResult<TResponseDto>(resource);
    }

    public virtual async Task<IOperationResult> UpdateAsync(
        Guid congregationId,
        Guid id,
        IPrimaryUpdateDto dto,
        CancellationToken ct = default
    )
    {
        var entity = Mapper.Map<T>(dto);
        entity.CongregationId = congregationId;
        entity.Id = id;

        var success = await Repository.UpdateRecord(id, congregationId, entity, ct);

        if (!success)
            return new NotFoundResult($"{nameof(entity)} with Id: {entity.Id} does not exist or has been deleted.");

        await UnitOfWork.CommitAsync(ct);
        return new OkResult("Record updated successfully.");
    }

    public virtual async Task<IOperationResult> DeleteAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var success = await Repository.DeleteRecord(id, congregationId, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        await UnitOfWork.CommitAsync(ct);
        return new OkResult("Record deleted successfully.");
    }
}
