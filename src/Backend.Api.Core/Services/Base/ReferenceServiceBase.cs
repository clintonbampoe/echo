using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Base;

namespace Backend.Api.Core.Services.Base;

public abstract class ReferenceServiceBase<T>(
    ReferenceRepositoryBase<T> repository,
    AppDbContext context,
    IMapper mapper
)
    where T : class, IReferenceEntity
{
    protected readonly ReferenceRepositoryBase<T> _repository = repository;
    protected readonly AppDbContext _context = context;
    protected readonly IMapper _mapper = mapper;

    public abstract Task<IOperationResult> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    );

    public abstract Task<IOperationResult> GetByIdAsync(int id, CancellationToken ct = default);

    public virtual async Task<IOperationResult> CreateAsync(
        IReferenceCreateDto<T> dto,
        CancellationToken ct = default
    )
    {
        var entity = _mapper.Map<T>(dto);
        await _repository.CreateRecord(entity, ct);
        await _context.SaveChangesAsync(ct);
        return new OkResult("Record created successfully.");
    }

    public virtual async Task<IOperationResult> UpdateAsync(
        int id,
        IReferenceUpdateDto<T> dto,
        CancellationToken ct = default
    )
    {
        var entity = _mapper.Map<T>(dto);
        var success = await _repository.UpdateRecord(id, entity, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        await _context.SaveChangesAsync(ct);
        return new OkResult("Record updated successfully.");
    }

    public virtual async Task<IOperationResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var success = await _repository.DeleteRecord(id, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        await _context.SaveChangesAsync(ct);
        return new OkResult("Record deleted successfully.");
    }
}
