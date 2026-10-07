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
                Returns the complete list of Asset Categories for the caller's congregation.

                ### No pagination
                The response is a plain array, not a paged envelope. Asset Categories are lightweight lookup entities bounded in number, so the full list is always returned.

                Call this to populate the category selector when creating or updating Assets.

                ### Failure modes
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
                Returns the Asset Category record for the given ID, scoped to the caller's congregation.

                ### ID is an integer
                Asset Category IDs are 32-bit integers, not UUIDs. Lookup entities in Echo use integer keys.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset category exists with the given ID in this congregation, or the record has been soft-deleted.
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
                    var result = await service.Create(
                        context.User.GetCongregationId(),
                        dto,
                        context,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("CreateAssetCategory")
            .WithSummary("Creates a new asset category.")
            .WithDescription(
                """
                Creates a new Asset Category — for example Equipment, Furniture, or Vehicles. Scoped to the caller's congregation.

                ### Request body
                - `name` — required. 1 to 100 characters.

                ### On success
                Returns `201 Created` with the full `AssetCategoryResponseDto` and a `Location` header pointing to `GET /asset-categories/{id}`.

                ### Side effects
                - The category becomes immediately available for assignment to Assets.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `name` failed validation.
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
                Updates the name of an Asset Category. Partial update.

                ### Side effects
                - **Existing Assets assigned to this category are not affected.** They retain their assignment and will display the new name in their `categoryName` field on next fetch.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `name` failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset category exists with the given ID in this congregation, or the record has been soft-deleted.
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
                Soft-deletes the Asset Category. The row is retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The category is marked as deleted.
                - **Existing Assets assigned to this category are not affected.** They keep their `categoryId` pointing at the (now deleted) category. On fetch, `categoryName` may resolve to null.
                - **New assets cannot reference a deleted category** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no asset category exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                Trigram-based similarity search over asset category names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive. Partial matches supported.

                ### On success
                Returns `200 OK` with a flat array of `AssetCategorySearchResultDto`, ranked by similarity. **No pagination.**

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<AssetCategorySearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
