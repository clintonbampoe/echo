using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Attendances;

public class AttendanceService(
    AttendanceRepository repository,
    AttendanceContextRepository contextRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IAttendanceMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        AttendanceFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.list");

        var cursor = encoder.Decode<AttendanceCursor>(pagination.Cursor);

        List<Attendance> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.list"))
        {
            entities = await repository.List(
                congregationId,
                filters,
                cursor,
                pagination.PageSize + 1,
                ct
            );
        }

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);
        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("attendance.count", data.Count);

        var res = new PagedResponse<AttendanceResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<AttendanceResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance.get_by_id"
        );
        activity?.SetTag("attendance.id", id);

        Attendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AttendanceCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.create");

        AttendanceContext? context;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.validate.context"))
        {
            context = await contextRepository.GetById(congregationId, dto.AttendanceContextId, ct);
        }

        if (context is null)
            return new ForeignKeyEntityNotFound(nameof(context));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.AttendanceContext = context;

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("attendance.id", entity.Id);

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
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.update");
        activity?.SetTag("attendance.id", id);

        Attendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<AttendanceResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.delete");
        activity?.SetTag("attendance.id", id);

        Attendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    private static AttendanceCursor BuildCursor(Attendance last)
    {
        return new AttendanceCursor { ForDate = last.ForDate, Id = last.Id };
    }
}
