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
                Returns a cursor-paginated list of Assets scoped to the caller's congregation.

                ### Filtering
                All filters are optional and combinable:
                - `status` — `Active`, `InUse`, `InStorage`, `UnderMaintenance`, `Liquidated`.
                - `categoryId` — filter to a specific Asset Category.
                - `name` — partial, case-insensitive match against the asset name.
                - `from` / `to` — filter by purchase-date range (inclusive).

                ### Ordering
                Assets are ordered by name, then by ID as a tiebreaker. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

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
                Returns the full Asset record for the given ID, scoped to the caller's congregation.

                The response includes the resolved `categoryName` — no follow-up call needed.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<AssetResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    AssetCreateDto dto,
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
                Creates a new Asset scoped to the caller's congregation.

                ### Request body
                Required fields:
                - `name` — 1 to 100 characters.
                - `categoryId` — must reference an Asset Category in the caller's congregation.
                - `purchaseCost` — 0 to 1,000,000.
                - `currentValue` — 0 to 1,000,000.
                - `status` — one of `Active`, `InUse`, `InStorage`, `UnderMaintenance`, `Liquidated`.

                Optional fields:
                - `serialNumber` — up to 100 characters.
                - `purchaseDate` — ISO date.
                - `description` — up to 2000 characters.

                ### On success
                Returns `201 Created` with the full `AssetResponseDto` and a `Location` header pointing to `GET /assets/{id}`.

                ### Depreciation
                The server does **not** compute depreciation for you. `purchaseCost` and `currentValue` are stored as supplied. The summary endpoint reports `totalDepreciation` as the difference between the two totals across matching assets — it does not track depreciation over time per asset.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced Asset Category does not exist in this congregation.
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
                    Guid id,
                    AssetUpdateDto dto,
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
                Updates the supplied fields on an existing Asset. Partial update — omitted fields retain their current values.

                ### Request body
                All fields optional. If `categoryId` is supplied, the new category must exist in the caller's congregation.

                ### On success
                Returns `200 OK` with the full updated `AssetResponseDto`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset exists with the given ID in this congregation, or the record has been soft-deleted.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced Asset Category does not exist in this congregation.
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
                Soft-deletes the Asset. The row is retained in the database but excluded from list, summary, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The asset is marked as deleted.
                - **Summary totals will reflect the deletion.** `GET /assets/summary` recalculates from live assets only.

                ### Reversibility
                Not exposed as reversible through the API.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/search",
                async (
                    [FromQuery] string q,
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
                Trigram-based similarity search over asset names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive. Partial matches supported.

                ### On success
                Returns `200 OK` with a flat array of `AssetSearchResultDto`, ranked by similarity. **No pagination.**

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
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
                Returns aggregated financial metrics for the caller's assets, optionally scoped by the same filters as the list endpoint.

                ### Response fields
                - `totalAssets` — count of assets matching the filter.
                - `totalCurrentValue` — sum of `currentValue` across matching assets.
                - `totalPurchaseCost` — sum of `purchaseCost` across matching assets.
                - `totalDepreciation` — `totalPurchaseCost - totalCurrentValue`.

                ### Depreciation is a snapshot, not a time series
                `totalDepreciation` is the arithmetic difference between two stored fields. It does not model depreciation schedules, useful life, or accounting depreciation. If you need those, compute them client-side or in a reporting layer.

                `totalDepreciation` can be negative if current values exceed purchase costs (appreciation).

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<AssetSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
