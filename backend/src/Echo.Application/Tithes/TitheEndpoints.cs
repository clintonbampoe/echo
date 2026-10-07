using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Tithes;

public static class TitheEndpoints
{
    public static IEndpointRouteBuilder MapTitheEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/tithes").WithTags("Tithes").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] TitheFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    TitheService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.tithe.list"
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
            .WithName("ListTithes")
            .WithSummary("Returns a paginated list of tithes.")
            .WithDescription(
                """
                Returns a cursor-paginated list of tithe records scoped to the authenticated congregation.

                ### Filtering
                Filter by member, payment method, or date range. All filters are optional and combinable.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<TitheResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, TitheService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.tithe.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetTitheById")
            .WithSummary("Returns a single tithe record by ID.")
            .WithDescription(
                """
                Returns the full tithe record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no tithe record exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<TitheResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    TitheCreateDto dto,
                    TitheService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.tithe.create"
                    );
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateTithe")
            .WithSummary("Records a new tithe.")
            .WithDescription(
                """
                Records a tithe payment made by a member. The member must exist within the same congregation.

                On success, returns `201 Created` with the full tithe record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member does not exist in this congregation.
                """
            )
            .Produces<TitheResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    TitheUpdateDto dto,
                    TitheService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.tithe.update"
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
            .WithName("UpdateTithe")
            .WithSummary("Updates an existing tithe record.")
            .WithDescription(
                """
                Replaces the fields of an existing tithe record. All updatable fields must be supplied.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no tithe record exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<TitheResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, TitheService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.tithe.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteTithe")
            .WithSummary("Soft deletes a tithe record.")
            .WithDescription(
                """
                Marks the tithe record as deleted. The record is retained in the database but excluded from all list and lookup results. Tithe summary totals will reflect the deletion.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no tithe record exists with the given ID in this congregation, or it has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] TitheFilters filters,
                    TitheService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.tithe.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetTitheSummary")
            .WithSummary("Returns aggregate summary metrics for tithes.")
            .WithDescription(
                """
                Returns aggregated tithe metrics for the congregation, optionally scoped by the same filters available on the list endpoint.

                ### Response includes
                - `totalTithes` — count of tithe records matching the filter.
                - `totalAmount` — sum of all tithe amounts matching the filter.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<TitheSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
