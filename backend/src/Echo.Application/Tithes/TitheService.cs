using Echo.Application.Caching;
using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Members;
using Echo.Domain.Tithes;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Echo.Shared.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
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
    DateWindows dateWindows,
    SummaryCache summaries,
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

    // A member has one tithe per month; topping up is an update of that tithe. The check below gives the friendly
    // answer, and the unique index in the database is what makes it hold when two requests race.
    internal const string DuplicateCode = "TITHE_DUPLICATE";

    internal static string DuplicateDetail(string memberName, MonthOfYear month, int year) =>
        $"{memberName} already has a tithe for {month} {year}. Update that tithe instead.";

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
        MemberTitheFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        // A member's own tithes have no default window: they are listed from the beginning.
        var window = dateWindows.ThisWeekToDate(filters.From, filters.To, hasParent: true);
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

        bool exists;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.validate.duplicate"))
        {
            exists = await repository.ExistsForMonth(
                congregationId,
                dto.MemberId,
                dto.ForYear,
                dto.ForMonth,
                null,
                ct
            );
        }
        if (exists)
        {
            TitheLog.CreateDuplicate(
                logger,
                congregationId,
                dto.MemberId,
                dto.ForMonth.ToString(),
                dto.ForYear
            );
            return new ConflictResult(
                DuplicateDetail(member.Person.Name, dto.ForMonth, dto.ForYear),
                DuplicateCode
            );
        }

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Member = member;

        using (instrumentation.ActivitySource.StartActivity("svc.tithe.persist"))
        {
            repository.Create(entity);
            try
            {
                await unitOfWork.CommitAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                // Another request recorded this member's tithe for the month between the check above and the save.
                TitheLog.CreateDuplicate(
                    logger,
                    congregationId,
                    dto.MemberId,
                    dto.ForMonth.ToString(),
                    dto.ForYear
                );
                return new ConflictResult(
                    DuplicateDetail(member.Person.Name, dto.ForMonth, dto.ForYear),
                    DuplicateCode
                );
            }
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

        // Moving a tithe to another month can land on one the member already has.
        var year = dto.ForYear ?? entity.ForYear;
        var month = dto.ForMonth ?? entity.ForMonth;
        var moved = year != entity.ForYear || month != entity.ForMonth;
        if (
            moved
            && await repository.ExistsForMonth(
                congregationId,
                entity.MemberId,
                year,
                month,
                entity.Id,
                ct
            )
        )
        {
            TitheLog.UpdateDuplicate(
                logger,
                congregationId,
                entity.MemberId,
                month.ToString(),
                year
            );
            return new ConflictResult(
                DuplicateDetail(entity.Member.Person.Name, month, year),
                DuplicateCode
            );
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.tithe.persist"))
        {
            try
            {
                await unitOfWork.CommitAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                TitheLog.UpdateDuplicate(
                    logger,
                    congregationId,
                    entity.MemberId,
                    month.ToString(),
                    year
                );
                return new ConflictResult(
                    DuplicateDetail(entity.Member.Person.Name, month, year),
                    DuplicateCode
                );
            }
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

    public Task<IOperationResult> Summary(
        Guid congregationId,
        TitheFilters filters,
        bool includePrevious,
        CancellationToken ct
    ) =>
        summaries.Serve(
            "tithes",
            congregationId,
            new { filters, includePrevious },
            token => ComputeSummary(congregationId, filters, includePrevious, token),
            ct
        );

    private async Task<TitheSummaryDto> ComputeSummary(
        Guid congregationId,
        TitheFilters filters,
        bool includePrevious,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.summary");

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var previousTotal = 0m;
        if (includePrevious && window is { } w)
        {
            var before = w.Previous();
            previousTotal = (
                await repository.GetSummaryStats(
                    congregationId,
                    filters with { From = before.From, To = before.To },
                    ct
                )
            ).Total;
        }

        var res = new TitheSummaryDto
        {
            Window = window,
            PreviousTotalCollected = previousTotal,
            TotalCollected = stats.Total,
            UniqueTithers = stats.UniqueTithers,
            MostUsedPaymentMethod = TitheStats.MostUsed(stats.ByMethod)?.ToString(),
            AveragePerMember = AveragePerMember(stats.Total, stats.UniqueTithers),
            LargestTithe = stats.Largest,
            ByMethod =
            [
                .. stats
                    .ByMethod.OrderByDescending(m => m.Count)
                    .ThenBy(m => m.Method)
                    .Select(m => new TitheMethodDto
                    {
                        Method = m.Method.ToString(),
                        Count = m.Count,
                        Total = m.Total,
                    }),
            ],
        };

        activity?.SetTag("tithe.summary.total", res.TotalCollected);
        TitheLog.Summarized(logger, congregationId, res.TotalCollected);

        return res;
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
        if (MonthRange.Problem(MonthRange.Or(filters.Months)) is { } problem)
            return new BadRequestResult(problem);

        return await summaries.Serve(
            "tithes.monthly",
            congregationId,
            filters,
            token => ComputeMonthly(congregationId, filters, token),
            ct
        );
    }

    private async Task<List<TitheMonthlyDto>> ComputeMonthly(
        Guid congregationId,
        TitheMonthlyFilters filters,
        CancellationToken ct
    )
    {
        var months = dateWindows.ChartMonths(MonthRange.Or(filters.Months), filters.To);
        var rows = await repository.GetMonthlyTotals(
            congregationId,
            filters,
            first: months[0],
            last: months[^1],
            ct
        );

        return MonthRange.Fill(
            months,
            rows,
            r => new YearMonth(r.Year, r.Month),
            m => new TitheMonthlyDto { Year = m.Year, Month = m.Month }
        );
    }
}
