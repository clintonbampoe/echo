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
                Returns a cursor-paginated list of Tithe records scoped to the caller's congregation.

                ### Filtering
                All filters are optional and combinable:
                - `memberId` — tithes given by one member.
                - `paymentMethod` — `Cash`, `Cheque`, `CreditCard`, `MobileMoney`, `BankTransfer`.
                - `year` — the year the tithe is *for* (1900–2100).
                - `month` — the month the tithe is *for* (enum of month names).
                - `from` / `to` — filter by the actual collection date range.

                ### Year/month vs collection date
                A tithe is *for* a period (`forYear` + `forMonth`) and is *collected* on a date (`collectionDate`). These can differ — a December tithe collected in January. Filtering by `year`/`month` matches the period; filtering by `from`/`to` matches the collection date. Pick the one that matches your reporting question.

                ### Ordering
                Tithes are ordered by collection date, most recent first. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
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
                Returns the full Tithe record for the given ID, scoped to the caller's congregation.

                The response includes the resolved `memberName` — no follow-up call needed.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no tithe record exists with the given ID in this congregation, or the record has been soft-deleted.
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
                    var result = await service.Create(
                        context.User.GetCongregationId(),
                        dto,
                        context,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("CreateTithe")
            .WithSummary("Records a new tithe.")
            .WithDescription(
                """
                Records a tithe payment made by a Member.

                ### Request body
                Required fields:
                - `memberId` — must reference a Member in the caller's congregation.
                - `amount` — 0.01 to 1,000,000.
                - `forYear` — 1900 to 2100. The year the tithe is *for*.
                - `forMonth` — the month the tithe is *for*.
                - `paymentMethod` — `Cash`, `Cheque`, `CreditCard`, `MobileMoney`, `BankTransfer`.
                - `collectionDate` — ISO date. When the tithe was physically received.

                Optional:
                - `description` — up to 2000 characters.

                **`forYear`/`forMonth` and `collectionDate` are independent.** A December tithe paid in January has `forMonth = December` and `collectionDate` in January. The client is responsible for populating both correctly — the server does not infer one from the other.

                ### On success
                Returns `201 Created` with the full `TitheResponseDto` and a `Location` header pointing to `GET /tithes/{id}`.

                ### Side effects
                - The tithe is recorded. `GET /tithes/summary` reflects it on next call.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member does not exist in this congregation.
                """
            )
            .Produces<TitheResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
                Updates the supplied fields on an existing Tithe. Partial update.

                ### Request body
                All fields optional: `memberId`, `amount`, `forYear`, `forMonth`, `paymentMethod`, `collectionDate`, `description`.

                If `memberId` is supplied, the new member is validated against the caller's congregation.

                ### On success
                Returns `200 OK` with the full updated `TitheResponseDto`.

                ### Side effects
                - If the amount changed, `GET /tithes/summary` recalculates on next call.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no tithe exists with the given ID in this congregation, or the record has been soft-deleted.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member does not exist in this congregation.
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
                Soft-deletes the Tithe. The row is retained in the database but excluded from list, summary, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The tithe is marked as deleted.
                - **Summary totals reflect the deletion.** `GET /tithes/summary` recalculates from live records only.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no tithe exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                Returns aggregated tithe metrics for the caller's congregation, optionally scoped by the same filters as the list endpoint.

                ### Response fields
                - `totalCollected` — sum of amounts across matching tithes.
                - `uniqueTithers` — count of distinct members who gave at least one matching tithe.
                - `mostUsedPaymentMethod` — the payment method with the most matching tithes, as a string. Null when no tithes match.
                - `averagePerMember` — `totalCollected / uniqueTithers`. Zero when there are no tithers.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<TitheSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
