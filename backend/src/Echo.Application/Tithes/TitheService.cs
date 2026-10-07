using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Members;
using Echo.Domain.Tithes;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Tithes;

public class TitheService(
    TitheRepository repository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    ITitheMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<TitheService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        TitheFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.list");

        var cursor = encoder.Decode<TitheCursor>(pagination.Cursor);

        List<Tithe> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.list"))
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
        activity?.SetTag("tithe.count", data.Count);
        TitheLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<TitheResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<TitheResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.get_by_id");
        activity?.SetTag("tithe.id", id);

        Tithe? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("tithe.found", false);
            TitheLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        TitheLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        TitheCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.create");

        Member? member;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.validate.member"))
        {
            member = await memberRepository.GetById(congregationId, dto.MemberId, ct);
        }
        if (member is null)
        {
            TitheLog.CreateMemberNotFound(logger, congregationId, dto.MemberId);
            return new ForeignKeyEntityNotFound(nameof(member));
        }
        TitheLog.CreateMemberFound(logger, congregationId, dto.MemberId);

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Member = member;

        using (instrumentation.ActivitySource.StartActivity("svc.tithe.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("tithe.id", entity.Id);
        TitheLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);

        var location =
            linker.GetPathByName(httpContext, "GetTitheById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetTitheById' is not registered.");
        return new CreatedResult<TitheResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        TitheUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.update");
        activity?.SetTag("tithe.id", id);

        Tithe? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("tithe.found", false);
            TitheLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        // Validate the NEW member, not the existing one.
        if (dto.MemberId.HasValue && dto.MemberId.Value != entity.MemberId)
        {
            Member? member;
            using (instrumentation.ActivitySource.StartActivity("svc.tithe.validate.member"))
            {
                member = await memberRepository.GetById(congregationId, dto.MemberId.Value, ct);
            }

            if (member is null)
            {
                TitheLog.UpdateMemberNotFound(logger, congregationId, dto.MemberId.Value);
                return new ForeignKeyEntityNotFound(nameof(member));
            }
            TitheLog.UpdateMemberFound(logger, congregationId, dto.MemberId.Value);
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.tithe.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        TitheLog.Updated(logger, congregationId, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.delete");
        activity?.SetTag("tithe.id", id);

        Tithe? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("tithe.found", false);
            TitheLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.tithe.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        TitheLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        TitheFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.summary");

        var totalTask = repository.SumCollected(congregationId, filters, ct);
        var tithersTask = repository.CountUniqueTithers(congregationId, filters, ct);
        var paymentMethodTask = repository.MostUsedPaymentMethod(congregationId, filters, ct);
        var averageTask = repository.AveragePerMember(congregationId, filters, ct);

        await Task.WhenAll(totalTask, tithersTask, paymentMethodTask, averageTask);

        var res = new TitheSummaryDto
        {
            TotalCollected = totalTask.Result,
            UniqueTithers = tithersTask.Result,
            MostUsedPaymentMethod = paymentMethodTask.Result?.ToString(),
            AveragePerMember = averageTask.Result,
        };

        activity?.SetTag("tithe.summary.total", res.TotalCollected);
        TitheLog.Summarized(logger, congregationId, res.TotalCollected);

        return new SuccessResult<TitheSummaryDto>(res);
    }

    private static TitheCursor BuildCursor(Tithe last)
    {
        return new TitheCursor { CollectionDate = last.CollectionDate, Id = last.Id };
    }
}
