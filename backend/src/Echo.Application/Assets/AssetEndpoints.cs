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
                Returns a cursor-paginated list of assets scoped to the authenticated congregation.

                ### Filtering
                All filter parameters are optional and combinable. Omitting them returns all active assets ordered by name.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<AssetResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

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
                Returns the full asset record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset exists with the given ID in this congregation, or it has been soft-deleted.
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
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateAsset")
            .WithSummary("Creates a new asset.")
            .WithDescription(
                """
                Creates a new asset record scoped to the authenticated congregation. The asset must reference an existing Asset Category within the same congregation.

                On success, returns `201 Created` with the full asset record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced Asset Category does not exist in this congregation.
                """
            )
            .Produces<AssetResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

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
                Replaces the fields of an existing asset record. All updatable fields must be supplied — this is a full replacement, not a partial update.

                If the `categoryId` is being changed, the new category must exist within the same congregation.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset exists with the given ID in this congregation, or it has been soft-deleted.
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
                Marks the asset as deleted. The record is retained in the database but excluded from all list, search, and lookup results. This operation is irreversible through the API.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset exists with the given ID in this congregation, or it has already been soft-deleted.
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
                Performs a trigram-based similarity search against asset names using `pg_trgm`. Results are ranked by similarity to the query string `q`. Returns a flat list — no pagination.

                This endpoint is rate-limited. Excessive requests will be rejected.

                ### Errors
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
                Returns aggregated financial metrics for the congregation's assets, optionally scoped by the same filters available on the list endpoint.

                ### Response includes
                - `totalAssets` — count of active assets matching the filter.
                - `totalCurrentValue` — sum of current values across matching assets.
                - `totalPurchaseCost` — sum of original purchase costs.
                - `totalDepreciation` — difference between total purchase cost and total current value.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<AssetSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
