using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Services.Encoders;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.AttendanceMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class AttendanceService(
    AttendanceRepository repository,
    AttendanceContextRepository contextRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IAttendanceMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        AttendanceFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var cursor = encoder.Decode<AttendanceCursor>(pagination.Cursor);

        var entities = await repository.List(
            congregationId,
            filters,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);
        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<AttendanceResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<AttendanceResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(id, congregationId, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AttendanceCreateDto dto,
        CancellationToken ct
    )
    {
        var context = await contextRepository.GetById(congregationId, dto.AttendanceContextId, ct);

        if (context is null)
            return new ForeignKeyEntityNotFound(nameof(context));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.AttendanceContext = context;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<AttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        AttendanceUpdateDto dto,
        CancellationToken ct
    )
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        repository.SoftDelete(entity);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }

    private AttendanceCursor BuildCursor(Attendance last)
    {
        return new AttendanceCursor { ForDate = last.ForDate, Id = last.Id };
    }
}
