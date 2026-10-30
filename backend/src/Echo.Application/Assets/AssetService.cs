using Echo.Application.Caching;
using Echo.Data;
using Echo.Domain.Assets;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Echo.Shared.Utilities;

namespace Echo.Application.Assets;

public class AssetService(
    AssetRepository repository,
    AssetCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IAssetMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    DateWindows dateWindows,
    TimeProvider timeProvider,
    SummaryCache summaries,
    ILogger<AssetService> logger
)
{
    // Liquidating an asset dates it; moving it back to any other status takes the date away again.
    internal static DateOnly? LiquidatedOn(AssetStatus status, DateOnly? current, DateOnly today) =>
        status == AssetStatus.Liquidated ? current ?? today : null;

    // The summary describes what the church owns now, so liquidated assets are left out unless
    // the caller asks for them.
    internal static AssetFilters SummaryFilters(AssetFilters filters) =>
        filters with { IncludeLiquidated = filters.IncludeLiquidated ?? false };

    internal static DateWindow? ResolveWindow(AssetFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisYear(
            filters.From,
            filters.To,
            hasParent: false
        );

    public async Task<IOperationResult> List(
        Guid congregationId,
        AssetFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.list");

        var cursor = encoder.Decode<AssetCursor>(pagination.Cursor);
        List<Asset> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.list"))
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
        AssetLog.Listed(logger, congregationId, data.Count);
        activity?.SetTag("asset.count", data.Count);

        var res = new PagedResponse<AssetResponseDto>(hasMore, nextCursor, data, window);
        return new SuccessResult<PagedResponse<AssetResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.get_by_id");

        activity?.SetTag("asset.id", id);
        Asset? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset.found", false);
            AssetLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        AssetLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<AssetResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        AssetCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.create");

        var entity = mapper.ToEntity(dto);

        AssetCategory? category;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.validate.category"))
        {
            category = await categoryRepository.GetById(congregationId, entity.CategoryId, ct);
        }

        if (category is null)
        {
            AssetLog.CreateCategoryNotFound(logger, congregationId, entity.CategoryId);
            return new ForeignKeyEntityNotFound(nameof(category));
        }

        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Category = category;
        entity.LiquidatedOn = LiquidatedOn(entity.Status, null, DateUtils.GetDateToday(timeProvider));
        AssetLog.CreateCategoryFound(logger, congregationId, entity.CategoryId);

        using (instrumentation.ActivitySource.StartActivity("svc.asset.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("asset.id", entity.Id);
        AssetLog.Created(logger, congregationId, entity.Id);
        var res = mapper.ToDto(entity);

        var location =
            linker.GetPathByName(httpContext, "GetAssetById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetAssetById' is not registered.");
        return new CreatedResult<AssetResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        AssetUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.update");

        activity?.SetTag("asset.id", id);
        Asset? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset.found", false);
            AssetLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        // Validate Category if it is being changed to a different one.
        // We validate the NEW category (from the DTO), not the existing one.
        if (dto.CategoryId.HasValue && dto.CategoryId.Value != entity.CategoryId)
        {
            AssetCategory? category;
            using (instrumentation.ActivitySource.StartActivity("svc.asset.validate.category"))
            {
                category = await categoryRepository.GetById(
                    congregationId,
                    dto.CategoryId.Value,
                    ct
                );
            }

            if (category is null)
            {
                AssetLog.UpdateCategoryNotFound(logger, congregationId, dto.CategoryId.Value);
                return new ForeignKeyEntityNotFound(nameof(category));
            }
            AssetLog.UpdateCategoryFound(logger, congregationId, dto.CategoryId.Value);
        }

        using (instrumentation.ActivitySource.StartActivity("svc.asset.persist"))
        {
            mapper.Patch(dto, entity);
            entity.LiquidatedOn = LiquidatedOn(
                entity.Status,
                entity.LiquidatedOn,
                DateUtils.GetDateToday(timeProvider)
            );
            await unitOfWork.CommitAsync(ct);
        }

        AssetLog.Updated(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<AssetResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.delete");
        activity?.SetTag("asset.id", id);

        Asset? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("asset.found", false);
            AssetLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.asset.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }
        AssetLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.search");

        activity?.SetTag("asset.query", name);
        List<Asset> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.asset.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        AssetLog.Searched(logger, congregationId, name, res.Count);
        activity?.SetTag("asset.count", res.Count);

        return new SuccessResult<List<AssetSearchResultDto>>(res);
    }

    public Task<IOperationResult> Summary(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct
    ) =>
        summaries.Serve(
            "assets",
            congregationId,
            filters,
            token => ComputeSummary(congregationId, filters, token),
            ct
        );

    private async Task<AssetSummaryDto> ComputeSummary(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };
        filters = SummaryFilters(filters);

        using var activity = instrumentation.ActivitySource.StartActivity("svc.asset.summary");

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new AssetSummaryDto
        {
            Window = window,
            TotalAssets = stats.Count,
            TotalCurrentValue = stats.CurrentValue,
            TotalPurchaseCost = stats.PurchaseCost,
            TotalDepreciation = stats.PurchaseCost - stats.CurrentValue,
            AverageAgeYears = stats.AverageAgeYears,
        };

        activity?.SetTag("asset.summary.total", res.TotalAssets);
        AssetLog.Summarized(logger, congregationId, res.TotalAssets);

        return res;
    }

    public Task<IOperationResult> CategorySummary(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct
    ) =>
        summaries.Serve(
            "assets.by-category",
            congregationId,
            filters,
            token => ComputeCategorySummary(congregationId, filters, token),
            ct
        );

    private async Task<List<AssetCategoryTotalDto>> ComputeCategorySummary(
        Guid congregationId,
        AssetFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = SummaryFilters(filters with { From = window?.From, To = window?.To });

        return await repository.GetCategoryTotals(congregationId, filters, ct);
    }

    public async Task<IOperationResult> MonthlySummary(
        Guid congregationId,
        AssetMonthlyFilters filters,
        CancellationToken ct
    )
    {
        if (MonthRange.Problem(MonthRange.Or(filters.Months)) is { } problem)
            return new BadRequestResult(problem);

        return await summaries.Serve(
            "assets.monthly",
            congregationId,
            filters,
            token => ComputeMonthly(congregationId, filters, token),
            ct
        );
    }

    private async Task<List<AssetMonthlyDto>> ComputeMonthly(
        Guid congregationId,
        AssetMonthlyFilters filters,
        CancellationToken ct
    )
    {
        var months = dateWindows.ChartMonths(MonthRange.Or(filters.Months), filters.To);
        var (from, to) = MonthRange.Bounds(months);
        var acquired = await repository.GetAcquiredByMonth(congregationId, filters.CategoryId, from, to, ct);
        var lost = await repository.GetLostByMonth(congregationId, filters.CategoryId, from, to, ct);

        return Monthly(months, acquired, lost);
    }

    // Every month in the range, with what was bought and what was lost in it. A month with neither is zeros.
    internal static List<AssetMonthlyDto> Monthly(
        IReadOnlyList<YearMonth> months,
        IEnumerable<AssetMonthRow> acquired,
        IEnumerable<AssetMonthRow> lost)
    {
        var acquiredIn = acquired.ToDictionary(r => new YearMonth(r.Year, r.Month));
        var lostIn = lost.ToDictionary(r => new YearMonth(r.Year, r.Month));

        return
        [
            .. months.Select(m =>
            {
                acquiredIn.TryGetValue(m, out var bought);
                lostIn.TryGetValue(m, out var gone);
                return new AssetMonthlyDto
                {
                    Year = m.Year,
                    Month = m.Month,
                    AcquiredCount = bought?.Count ?? 0,
                    AcquiredCost = bought?.Cost ?? 0m,
                    LostCount = gone?.Count ?? 0,
                    LostCost = gone?.Cost ?? 0m,
                };
            }),
        ];
    }

    private static AssetCursor BuildCursor(Asset last)
    {
        return new AssetCursor { Name = last.Name, Id = last.Id };
    }
}
