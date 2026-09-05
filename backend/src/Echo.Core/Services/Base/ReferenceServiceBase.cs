using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Core.Dtos.Interfaces;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core.Interfaces;

namespace Echo.Core.Services.Base;

public abstract class ReferenceServiceBase<T, TResponseDto>(
    ReferenceRepositoryBase<T> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
)
    where T : class, IReferenceEntity
    where TResponseDto : IReferenceResponseDto
{
    protected readonly ReferenceRepositoryBase<T> Repository = repository;
    protected readonly IUnitOfWork UnitOfWork = unitOfWork;
    protected readonly IMapper Mapper = mapper;

    public abstract Task<IOperationResult> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    );

    public abstract Task<IOperationResult> GetByIdAsync(
        int id,
        Guid congregationId,
        CancellationToken ct = default
    );

    public virtual async Task<IOperationResult> CreateAsync(
        Guid congregationId,
        IReferenceCreateDto dto,
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
        int id,
        IReferenceUpdateDto dto,
        CancellationToken ct = default
    )
    {
        var entity = Mapper.Map<T>(dto);
        entity.Id = id;
        entity.CongregationId = congregationId;

        var success = await Repository.UpdateRecord(id, congregationId, entity, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        await UnitOfWork.CommitAsync(ct);
        return new OkResult("Record updated successfully.");
    }

    public virtual async Task<IOperationResult> DeleteAsync(
        int id,
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
