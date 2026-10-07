using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Assets;

public static class AssetCategoryEndpoints
{
    public static IEndpointRouteBuilder MapAssetCategoryEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/asset-categories")
            .WithTags("Asset Categories")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (AssetCategoryService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset_category.list"
                    );
                    var result = await service.List(context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("ListAssetCategories")
            .WithSummary("Returns all asset categories for the congregation.")
            .WithDescription(
                """
                Returns the complete list of asset categories for the authenticated congregation. Asset categories are lightweight lookup entities — no pagination is applied and the full list is always returned.

                Fetch this list to populate category selectors when creating or updating assets.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<AssetCategoryResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:int}",
                async (
                    int id,
                    AssetCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset_category.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetAssetCategoryById")
            .WithSummary("Returns a single asset category by ID.")
            .WithDescription(
                """
                Returns the asset category record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset category exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<AssetCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    AssetCategoryCreateDto dto,
                    AssetCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset_category.create"
                    );
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateAssetCategory")
            .WithSummary("Creates a new asset category.")
            .WithDescription(
                """
                Creates a new asset category scoped to the authenticated congregation. Once created, the category becomes available for assignment to assets.

                On success, returns `201 Created` with the full category record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<AssetCategoryResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:int}",
                async (
                    int id,
                    AssetCategoryUpdateDto dto,
                    AssetCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset_category.update"
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
            .WithName("UpdateAssetCategory")
            .WithSummary("Updates an existing asset category.")
            .WithDescription(
                """
                Replaces the fields of an existing asset category. All existing assets assigned to this category retain their assignment after the update.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset category exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<AssetCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:int}",
                async (
                    int id,
                    AssetCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset_category.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteAssetCategory")
            .WithSummary("Soft deletes an asset category.")
            .WithDescription(
                """
                Marks the asset category as deleted. The record is retained in the database but excluded from all list, search, and lookup results. Existing assets that reference this category are not affected.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset category exists with the given ID in this congregation, or it has already been soft-deleted.
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
                    AssetCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset_category.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchAssetCategories")
            .WithSummary("Searches asset categories by name.")
            .WithDescription(
                """
                Performs a trigram-based similarity search against asset category names using `pg_trgm`. Results are ranked by similarity to the query string `q`. Returns a flat list — no pagination.

                This endpoint is rate-limited. Excessive requests will be rejected.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<AssetCategorySearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
