using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos.Core.Interfaces;
using Api.Core.Entities.Core.Interfaces;
using Api.Core.Repositories.Base;
using AutoMapper;

namespace Api.Core.Services.Base;

public abstract class PrimaryServiceBase<T>(
    PrimaryRepositoryBase<T> repository,
    AppDbContext context,
    IMapper mapper
)
    where T : class, IPrimaryEntity
{
    protected readonly PrimaryRepositoryBase<T> _repository = repository;
    protected readonly AppDbContext _context = context;
    protected readonly IMapper _mapper = mapper;

    public abstract Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    );

    public abstract Task<IOperationResult> GetByIdAsync(Guid Id, CancellationToken ct = default);

    public virtual async Task<IOperationResult> CreateAsync(
        Guid congregationId,
        IPrimaryCreateDto dto,
        CancellationToken ct = default
    )
    {
        var entity = _mapper.Map<T>(dto);
        entity.CongregationId = congregationId;

        await _repository.CreateRecord(entity, ct);
        await _context.SaveChangesAsync(ct);
        return new OkResult("Record created successfully.");
    }

    public virtual async Task<IOperationResult> UpdateAsync(
        Guid congregationId,
        Guid id,
        IPrimaryUpdateDto dto,
        CancellationToken ct = default
    )
    {
        var entity = _mapper.Map<T>(dto);
        entity.CongregationId = congregationId;

        var success = await _repository.UpdateRecord(id, entity, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        await _context.SaveChangesAsync(ct);
        return new OkResult("Record updated successfully.");
    }

    public virtual async Task<IOperationResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var success = await _repository.DeleteRecord(id, ct);

        if (!success)
            return new NotFoundResult("Record not found.");

        await _context.SaveChangesAsync(ct);
        return new OkResult("Record deleted successfully.");
    }
}
