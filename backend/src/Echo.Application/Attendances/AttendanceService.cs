using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Attendances;
using Echo.Domain.Members;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

public class AttendanceService(
    AttendanceRepository repository,
    AttendanceTypeRepository serviceTypeRepository,
    PersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IAttendanceMapper mapper,
    IEncoder encoder,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    ILogger<AttendanceService> logger
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
        AttendanceLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<AttendanceResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<AttendanceResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.attendance.get_by_id"
        );
        activity?.SetTag("attendance.id", id);

        Attendance? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            AttendanceLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        AttendanceLog.Found(logger, congregationId, id);
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

        AttendanceType? serviceType;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.validate.service_type"))
        {
            serviceType = await serviceTypeRepository.GetById(
                congregationId,
                dto.AttendanceTypeId,
                ct
            );
        }
        if (serviceType is null)
        {
            AttendanceLog.CreateServiceTypeNotFound(logger, congregationId, dto.AttendanceTypeId);
            return new ForeignKeyEntityNotFound(nameof(serviceType));
        }
        AttendanceLog.CreateServiceTypeFound(logger, congregationId, dto.AttendanceTypeId);

        Person? person;
        using (instrumentation.ActivitySource.StartActivity("svc.attendance.validate.person"))
        {
            person = await personRepository.GetById(congregationId, dto.PersonId, ct);
        }
        if (person is null)
        {
            AttendanceLog.CreatePersonNotFound(logger, congregationId, dto.PersonId);
            return new ForeignKeyEntityNotFound(nameof(person));
        }
        AttendanceLog.CreatePersonFound(logger, congregationId, dto.PersonId);

        var entity = mapper.ToEntity(dto);
        entity.Id = idGenerator.Generate();
        entity.CongregationId = congregationId;
        entity.AttendanceType = serviceType;
        entity.Person = person;

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("attendance.id", entity.Id);
        AttendanceLog.Created(logger, congregationId, entity.Id);

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
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            AttendanceLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            mapper.Patch(dto, entity);
            await unitOfWork.CommitAsync(ct);
        }

        AttendanceLog.Updated(logger, congregationId, id);

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
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("attendance.found", false);
            AttendanceLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.attendance.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        AttendanceLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        AttendanceFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.attendance.summary");

        var totalTask = repository.CountTotal(congregationId, filters, ct);
        var membersTask = repository.CountByKind(congregationId, PersonKind.Member, filters, ct);
        var visitorsTask = repository.CountByKind(congregationId, PersonKind.Visitor, filters, ct);
        var firstTimeTask = repository.CountFirstTimeVisitors(congregationId, filters, ct);

        await Task.WhenAll(totalTask, membersTask, visitorsTask, firstTimeTask);

        var res = new AttendanceSummaryDto
        {
            TotalPresent = totalTask.Result,
            MembersPresent = membersTask.Result,
            VisitorsPresent = visitorsTask.Result,
            FirstTimeVisitors = firstTimeTask.Result,
        };

        activity?.SetTag("attendance.summary.total", res.TotalPresent);
        AttendanceLog.Summarized(logger, congregationId, res.TotalPresent);

        return new SuccessResult<AttendanceSummaryDto>(res);
    }

    private static AttendanceCursor BuildCursor(Attendance last) =>
        new AttendanceCursor { Date = last.Date, Id = last.Id };
}
