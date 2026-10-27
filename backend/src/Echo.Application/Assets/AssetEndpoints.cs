using System.ComponentModel.DataAnnotations;
using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Assets;

public static class AssetEndpoints
{
    public static IEndpointRouteBuilder MapAssetEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/assets").WithTags("Assets").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] AssetFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    AssetService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.list"
                    );
                    var result = await service.List(
                        context.User.GetCongregationId(),
                        filters,
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListAssets")
            .WithSummary("Returns a paginated list of assets.")
            .WithDescription(
                """
                Returns the church's assets, ordered by name, one page at a time.

                ### Filters
                All optional and combinable.
                - `name` — only assets whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `categoryId` — only assets in this category. Ids come from `GET /asset-categories`.
                - `status` — only assets with this status: `Active`, `InUse`, `InStorage`, `UnderMaintenance` or `Liquidated`. Asking for `Liquidated` shows liquidated assets whatever `includeLiquidated` says.
                - `includeLiquidated` — whether liquidated (sold or scrapped) assets are included. The **list** shows them unless you send `false`; the **summaries** leave them out unless you send `true`, because their totals describe what the church owns now.
                - `from`, `to` — only assets bought between these two days, inclusive (by `purchaseDate`). **With no dates, the window is this year**, so you get this year's purchases only. An asset with no purchase date is never inside a date window. To see everything, send a wide window such as `from=2000-01-01`.

                ### Ordering
                Always by `name`, then by id for assets with the same name. The order is fixed, so pages never skip or repeat a row.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### Response
                A page `{ hasMore, next, data, window }`. Each asset carries `categoryName`, `status`, `purchaseCost`, `currentValue` and `liquidatedOn` (the day it was liquidated; `null` while the church still owns it). `window` echoes the dates that were applied.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<AssetResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, AssetService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetAssetById")
            .WithSummary("Returns a single asset by ID.")
            .WithDescription(
                """
                Returns one asset, including its resolved `categoryName`, so no second request is needed to show it.

                ### Path parameters
                - `id` — the asset's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset has this id in the congregation, or it has been deleted.
                """
            )
            .Produces<AssetResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] AssetCreateDto dto,
                    AssetService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.create"
                    );
                    var result = await service.Create(
                        context.User.GetCongregationId(),
                        dto,
                        context,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("CreateAsset")
            .WithSummary("Creates a new asset.")
            .WithDescription(
                """
                Adds an asset to the church's register.

                ### Request body
                Required:
                - `name` — what the asset is called, 1 to 100 characters. Must be unique in the congregation.
                - `categoryId` — the category it belongs to. Must exist in the congregation (`GET /asset-categories`).
                - `purchaseCost` — what it cost, from 0 to 1,000,000.
                - `currentValue` — what it is worth now, from 0 to 1,000,000. Typed in by hand: nothing depreciates it for you.
                - `status` — `Active`, `InUse`, `InStorage`, `UnderMaintenance` or `Liquidated`.

                Optional:
                - `serialNumber` — up to 100 characters.
                - `purchaseDate` — the day it was bought (`2026-03-12`). Without it the asset is left out of age figures and of every purchase-date window.
                - `description` — up to 2000 characters.

                Creating an asset as `Liquidated` dates it today (`liquidatedOn`).

                ### Response
                `201 Created` with the asset and a `Location` header pointing to `GET /assets/{id}`.

                ### Failure modes
                - `400 VALIDATION_ERROR` — a field is missing or out of range; the `errors` object says which.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — `categoryId` is not a category of this congregation.
                - `409 CONFLICT` — another asset already has this `name`.
                """
            )
            .Produces<AssetResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] AssetUpdateDto dto,
                    AssetService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.update"
                    );
                    var result = await service.Update(
                        context.User.GetCongregationId(),
                        id,
                        dto,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("UpdateAsset")
            .WithSummary("Updates an existing asset.")
            .WithDescription(
                """
                Changes the fields you send; fields you leave out keep their value.

                ### Path parameters
                - `id` — the asset's id, as returned by the list and create endpoints.

                ### Request body
                All optional; send only what changed.
                - `name` — the new name, 1 to 100 characters. Must stay unique in the congregation.
                - `categoryId` — move it to another category; the category must exist in the congregation.
                - `serialNumber` — up to 100 characters.
                - `purchaseDate` — the day it was bought. It can be changed but not taken away.
                - `purchaseCost`, `currentValue` — 0 to 1,000,000.
                - `status` — `Active`, `InUse`, `InStorage`, `UnderMaintenance` or `Liquidated`. Moving an asset to `Liquidated` dates it today (`liquidatedOn`); moving it back to any other status clears the date.
                - `description` — up to 2000 characters.

                ### Response
                `200 OK` with the updated asset.

                ### Failure modes
                - `400 VALIDATION_ERROR` — a field is out of range; the `errors` object says which.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset has this id in the congregation, or it has been deleted.
                - `404 FOREIGN_KEY_NOT_FOUND` — `categoryId` is not a category of this congregation.
                - `409 CONFLICT` — another asset already has this `name`.
                """
            )
            .Produces<AssetResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, AssetService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteAsset")
            .WithSummary("Soft deletes an asset.")
            .WithDescription(
                """
                Removes an asset from the register. The record is kept but no longer appears in lists, searches or totals.

                ### Path parameters
                - `id` — the asset's id, as returned by the list and create endpoints.

                ### Response
                `204 No Content`. Totals from `GET /assets/summary` and its siblings leave the asset out from then on (within the 2-minute cache). Deleting is not reversible through the API.

                If the asset was sold or scrapped rather than entered by mistake, set its status to `Liquidated` instead: that keeps it in the history and in the "lost" chart.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset has this id in the congregation, or it was already deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/search",
                async (
                    [FromQuery, StringLength(100, MinimumLength = 1)] string q,
                    AssetService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchAssets")
            .WithSummary("Searches assets by name.")
            .WithDescription(
                """
                Finds assets by name as you type, ranked by how closely the name matches. For a pick-list or a search box.

                ### Query parameters
                - `q` — the text to look for, 1 to 100 characters. Case-insensitive; partial words and small typos still match.

                ### Response
                A flat list of `{ id, name }`, best match first, at most a handful of rows. It is not paged. To open one, use its `id` with `GET /assets/{id}`.

                ### Rate limiting
                Limited by the `search` policy; slow down if you receive `429`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<AssetSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] AssetFilters filters,
                    AssetService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetAssetSummary")
            .WithSummary("Returns aggregate summary metrics for assets.")
            .WithDescription(
                """
                Returns the headline numbers for the Assets page, for the assets the filters pick.

                ### Query parameters
                Same meaning as on the list, so the numbers describe the list the page is showing.
                - `name` — only assets whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `categoryId` — only assets in this category. Ids come from `GET /asset-categories`.
                - `status` — only assets with this status: `Active`, `InUse`, `InStorage`, `UnderMaintenance` or `Liquidated`. Asking for `Liquidated` shows liquidated assets whatever `includeLiquidated` says.
                - `includeLiquidated` — whether liquidated (sold or scrapped) assets are included. The **list** shows them unless you send `false`; the **summaries** leave them out unless you send `true`, because their totals describe what the church owns now.
                - `from`, `to` — only assets bought between these two days, inclusive (by `purchaseDate`). **With no dates, the window is this year**, so you get this year's purchases only. An asset with no purchase date is never inside a date window. To see everything, send a wide window such as `from=2000-01-01`.

                ### Response
                - `totalAssets` — how many assets match.
                - `totalCurrentValue` — their `currentValue` added up.
                - `totalPurchaseCost` — their `purchaseCost` added up.
                - `totalDepreciation` — `totalPurchaseCost` minus `totalCurrentValue`. It is a snapshot of two stored numbers, not a depreciation schedule, and is negative when assets are worth more than they cost.
                - `averageAgeYears` — how old the assets are on average, in years, over those that have a purchase date. `0` when none does.
                - `window` — the dates that were applied.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<AssetSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary/by-category",
                async (
                    [AsParameters] AssetFilters filters,
                    AssetService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.category_summary"
                    );
                    var result = await service.CategorySummary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetAssetCategorySummary")
            .WithSummary("Returns what the assets of each category cost and are worth.")
            .WithDescription(
                """
                Returns the assets grouped by category, so the Assets page can show where the value is (a donut, bars, a legend) from a single request.

                ### Query parameters
                Same meaning as on `GET /assets/summary`, so the rows add up to that summary's totals.
                - `name` — only assets whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `categoryId` — only assets in this category. Ids come from `GET /asset-categories`.
                - `status` — only assets with this status: `Active`, `InUse`, `InStorage`, `UnderMaintenance` or `Liquidated`. Asking for `Liquidated` shows liquidated assets whatever `includeLiquidated` says.
                - `includeLiquidated` — whether liquidated (sold or scrapped) assets are included. The **list** shows them unless you send `false`; the **summaries** leave them out unless you send `true`, because their totals describe what the church owns now.
                - `from`, `to` — only assets bought between these two days, inclusive (by `purchaseDate`). **With no dates, the window is this year**, so you get this year's purchases only. An asset with no purchase date is never inside a date window. To see everything, send a wide window such as `from=2000-01-01`.

                ### Response
                A list of `{ categoryId, categoryName, count, totalPurchaseCost, totalCurrentValue, totalDepreciation }`, the category worth the most first. A category with no matching assets is left out.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<AssetCategoryTotalDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary/monthly",
                async (
                    [AsParameters] AssetMonthlyFilters filters,
                    AssetService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset.monthly_summary"
                    );
                    var result = await service.MonthlySummary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetAssetMonthlySummary")
            .WithSummary("Returns assets acquired and lost per month.")
            .WithDescription(
                """
                Returns one row per calendar month, so the Assets page can chart what the church bought against what it lost from a single request.

                An asset is **acquired** in the month of its `purchaseDate` and **lost** in the month it was liquidated (`liquidatedOn`). Assets with no purchase date are not in the acquired figures. Assets liquidated before liquidation dates were recorded have no `liquidatedOn` and are not in the lost figures.

                ### Query parameters
                - `months` — how many months to return, from 1 to 12. Defaults to 12. The rows end with the month of `to`, so `months=6` is the last six months.
                - `to` — any day in the last month to return. Defaults to today, so the last row is this month so far (a part month). Send an earlier date to look back in time: `months=6&to=2026-03-15` returns October 2025 to March 2026.
                - `categoryId` — only assets of this category.

                ### Response
                A list of `{ year, month, acquiredCount, acquiredCost, lostCount, lostCost }`. Costs are purchase costs: what the assets cost, not what they were worth. Oldest month first. A month with nothing in it is returned with zeros, so there are no gaps and the list always has exactly `months` rows.

                ### Why it is one endpoint
                This is the only monthly endpoint for assets: it serves a dashboard card (`months=6`), a full-year chart, or a donut over the months. The shape is always the same list, so the page decides how to draw it.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters, so asking again straight away costs nothing. A change shows up within 2 minutes.

                ### Failure modes
                - `400 BAD_REQUEST` — `months` is not between 1 and 12.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<AssetMonthlyDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
