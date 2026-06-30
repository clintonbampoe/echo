using AutoMapper;
using Echo.Core.Dtos.Interfaces;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core.Interfaces;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;

namespace Echo.Core.Services.Base;

public abstract class ReferenceServiceBase<T>(
    ReferenceRepositoryBase<T> repository,
    AppDbContext context,
    IMapper mapper
)
    where T : class, IReferenceEntity
{
    protected readonly ReferenceRepositoryBase<T> Repository = repository;
    protected readonly AppDbContext Context = context;
    protected readonly IMapper Mapper = mapper;

    public abstract Task<IOperationResult> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    );

    public abstract Task<IOperationResult> GetByIdAsync(int id, CancellationToken ct = default);

    public virtual async Task<IOperationResult> CreateAsync(
        Guid congregationId,
        IReferenceCreateDto dto,
        CancellationToken ct = default
    )
    {
        var entity = Mapper.Map<T>(dto);
        entity.CongregationId = congregationId;

        await Repository.CreateRecord(entity, ct);
        await Context.SaveChangesAsync(ct);
        return new OkResult("Record created successfully.");
    }

    public virtual async Task<IOperationResult> UpdateAsync(
        Guid congregationId,
        int id,
        IReferenceUpdateDto dto,
        CancellationToken ct = default
    )
    {
        var entity = Mapper.Map<T>(dto);
        entity.CongregationId = congregationId;

        var success = await Repository.UpdateRecord(id, entity, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        await Context.SaveChangesAsync(ct);
        return new OkResult("Record updated successfully.");
    }

    public virtual async Task<IOperationResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var success = await Repository.DeleteRecord(id, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        await Context.SaveChangesAsync(ct);
        return new OkResult("Record deleted successfully.");
    }
}
