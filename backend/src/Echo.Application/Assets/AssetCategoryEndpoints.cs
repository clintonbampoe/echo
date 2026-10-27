using System.ComponentModel.DataAnnotations;
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
                Returns every asset category, each with how many assets it holds and its icon. For the category pick-list and the Categories page.

                ### Parameters
                None. The list is not paged: categories are a short lookup list, so all of them are always returned.

                ### Response
                A list of `{ id, name, icon, assetCount }`. `icon` is the key the page shows the category with, or `null` when none was set. `assetCount` counts live assets only.

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
                    [FromRoute] int id,
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
                Returns one asset category with its icon and asset count.

                ### Path parameters
                - `id` — the category's id, a whole number (categories use integer ids, not GUIDs), as returned by `GET /asset-categories`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no category has this id in the congregation, or it has been deleted.
                """
            )
            .Produces<AssetCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] AssetCategoryCreateDto dto,
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
                Adds a category assets can be filed under, for example Equipment, Furniture or Vehicles.

                ### Request body
                - `name` — required, 1 to 100 characters. Must be unique in the congregation.
                - `icon` — optional, up to 40 characters. A key the page uses to pick the category's icon; the API stores it as given and does not check it.

                ### Response
                `201 Created` with the category and a `Location` header pointing to `GET /asset-categories/{id}`. It can be used for assets straight away.

                ### Failure modes
                - `400 VALIDATION_ERROR` — `name` or `icon` is missing or too long.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `409 CONFLICT` — a category with this name already exists.
                """
            )
            .Produces<AssetCategoryResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:int}",
                async (
                    [FromRoute] int id,
                    [FromBody] AssetCategoryUpdateDto dto,
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
                Renames a category or changes its icon. Send only what changed.

                ### Path parameters
                - `id` — the category's id, a whole number, as returned by `GET /asset-categories`.

                ### Request body
                - `name` — the new name, 1 to 100 characters. Must stay unique in the congregation.
                - `icon` — the new icon key, up to 40 characters.

                Assets in the category keep their link to it and show the new name the next time they are fetched.

                ### Failure modes
                - `400 VALIDATION_ERROR` — `name` or `icon` is too long or empty.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no category has this id in the congregation, or it has been deleted.
                - `409 CONFLICT` — another category already has this name.
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
                    [FromRoute] int id,
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
                Removes an empty category.

                ### Path parameters
                - `id` — the category's id, a whole number, as returned by `GET /asset-categories`.

                ### Response
                `204 No Content`. The category disappears from lists and can no longer be chosen for assets.

                A category that still has assets cannot be deleted: move the assets first with `POST /asset-categories/{id}/reassign`, which can also delete the category in the same step.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no category has this id in the congregation, or it was already deleted.
                - `409 CONFLICT` — assets still use the category.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/search",
                async (
                    [FromQuery, StringLength(100, MinimumLength = 1)] string q,
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
                Finds categories by name as you type, ranked by how closely the name matches.

                ### Query parameters
                - `q` — the text to look for, 1 to 100 characters. Case-insensitive; partial words and small typos still match.

                ### Response
                A flat list of `{ id, name }`, best match first. It is not paged.

                ### Rate limiting
                Limited by the `search` policy; slow down if you receive `429`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<AssetCategorySearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/{id:int}/reassign",
                async (
                    [FromRoute] int id,
                    [FromBody] ReassignCategoryDto dto,
                    AssetCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.asset_category.reassign"
                    );
                    var result = await service.Reassign(
                        context.User.GetCongregationId(),
                        id,
                        dto,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ReassignAssetCategory")
            .WithSummary("Moves all assets of an asset category to another category.")
            .WithDescription(
                """
                Moves every live asset from this category to another one in a single step, so a category can be merged away without ever leaving assets uncategorised. Deleted assets stay where they are.

                ### Path parameters
                - `id` — the category to move assets **out of**, a whole number, as returned by `GET /asset-categories`.

                ### Request body
                - `targetCategoryId` — the category to move the assets **into**. Must be a different, live category of the congregation.
                - `deleteSource` — `true` to also delete the source category in the same step (a merge); `false` (the default) to keep it, now empty.

                ### Response
                `200 OK` with `{ "moved": n, "sourceDeleted": true|false }`. `moved` is `0` when the category was empty.

                ### Failure modes
                - `400 BAD_REQUEST` — `targetCategoryId` is the same as `id`.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — the source category does not exist or is deleted.
                - `404 FOREIGN_KEY_NOT_FOUND` — the target category does not exist or is deleted.
                """
            )
            .Produces<ReassignCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
