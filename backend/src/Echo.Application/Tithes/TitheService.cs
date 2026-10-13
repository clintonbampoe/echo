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
using Echo.Shared.Utilities;

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
    DateWindows dateWindows,
    ILogger<TitheService> logger
)
{
    // The average a tither gave over the period: the total over the people who gave.
    internal static decimal AveragePerMember(decimal total, int uniqueTithers) =>
        uniqueTithers == 0 ? 0m : total / uniqueTithers;

    internal static DateWindow? ResolveWindow(TitheFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisWeekToDate(
            filters.From,
            filters.To,
            hasParent: filters.MemberId is not null
                || filters.Year is not null
                || filters.Month is not null
        );

    public async Task<IOperationResult> List(
        Guid congregationId,
        TitheFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

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

        var res = new PagedResponse<TitheResponseDto>(hasMore, nextCursor, data, window);
        return new SuccessResult<PagedResponse<TitheResponseDto>>(res);
    }

    public async Task<IOperationResult> GetByMemberId(
        Guid congregationId,
        Guid memberId,
        TitheFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters with { MemberId = memberId }, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.get.by_member_id");
        var cursor = encoder.Decode<TitheCursor>(pagination.Cursor);

        List<Tithe> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.by_member_id"))
        {
            entities = await repository.GetByMemberId(
                congregationId,
                memberId,
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
        TitheLog.ListByMemberId(logger, congregationId, memberId, data.Count);

        var res = new PagedResponse<TitheResponseDto>(hasMore, nextCursor, data, window);
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
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.summary");

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new TitheSummaryDto
        {
            Window = window,
            TotalCollected = stats.Total,
            UniqueTithers = stats.UniqueTithers,
            MostUsedPaymentMethod = stats.MostUsedPaymentMethod?.ToString(),
            AveragePerMember = AveragePerMember(stats.Total, stats.UniqueTithers),
        };

        activity?.SetTag("tithe.summary.total", res.TotalCollected);
        TitheLog.Summarized(logger, congregationId, res.TotalCollected);

        return new SuccessResult<TitheSummaryDto>(res);
    }

    private static TitheCursor BuildCursor(Tithe last)
    {
        return new TitheCursor { CollectionDate = last.CollectionDate, Id = last.Id };
    }

    public async Task<IOperationResult> MonthlySummary(
        Guid congregationId,
        TitheMonthlyFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.monthly_summary");

        var window = dateWindows.LastTwelveMonths(filters.From, filters.To, hasParent: false)!.Value;
        var months = MonthRange.Between(window.From, window.To);
        if (MonthRange.Problem(months) is { } problem)
            return new BadRequestResult(problem);

        var rows = await repository.GetMonthlyTotals(
            congregationId,
            filters,
            first: months[0],
            last: months[^1],
            ct
        );

        return new SuccessResult<List<TitheMonthlyDto>>(FillMonths(months, rows));
    }

    // Months without tithes are returned as zeros so a chart has no gaps.
    internal static List<TitheMonthlyDto> FillMonths(
        IReadOnlyList<YearMonth> months,
        IReadOnlyList<TitheMonthlyDto> rows
    ) =>
        months
            .Select(m =>
                rows.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)
                ?? new TitheMonthlyDto { Year = m.Year, Month = m.Month }
            )
            .ToList();
}
