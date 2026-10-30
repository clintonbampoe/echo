using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                - `memberName` — keep only tithes from members whose name contains this text (case-insensitive, matched anywhere in the name). Blank is ignored. Also narrows `/tithes/summary`.
                - `paymentMethod` — `Cash`, `Cheque`, `CreditCard`, `MobileMoney`, `BankTransfer`.
                - `year` — the year the tithe is *for* (1900–2100).
                - `month` — the month the tithe is *for* (enum of month names).
                - `from` / `to` — filter by the actual collection date range.

                ### Year/month vs collection date
                A tithe is *for* a period (`forYear` + `forMonth`) and is *collected* on a date (`collectionDate`). These can differ — a December tithe collected in January. Filtering by `year`/`month` matches the period; filtering by `from`/`to` matches the collection date. Pick the one that matches your reporting question.

                ### Ordering
                Tithes are ordered by collection date, most recent first. Stable across pages.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<TitheResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/member/{memberId:guid}",
                async (
                    [FromRoute] Guid memberId,
                    [AsParameters] MemberTitheFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    TitheService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.tithe.get_by_member_id"
                    );
                    var result = await service.GetByMemberId(
                        context.User.GetCongregationId(),
                        memberId,
                        filters,
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetTithesByMemberId")
            .WithSummary("Returns a paginated list of tithes for a specific member.")
            .WithDescription(
                """
                Returns a cursor-paginated list of Tithe records given by a single Member,
                scoped to the caller's congregation.

                If the member has no tithes — or the member ID does not exist in this
                congregation — the response is `200 OK` with an empty `items` list and
                `hasMore = false`. The endpoint does not distinguish between "no such
                member" and "member with no tithes."

                ### Filtering
                Combines with the same filters as the list endpoint:
                - `paymentMethod` — `Cash`, `Cheque`, `CreditCard`, `MobileMoney`, `BankTransfer`.
                - `year` — the year the tithe is *for* (1900–2100).
                - `month` — the month the tithe is *for*.
                - `from` / `to` — filter by the actual collection date range.

                `memberId` is taken from the path, not the query string.

                ### Ordering
                Tithes are ordered by collection date, most recent first. Stable across pages.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

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

                ### Path parameters
                - `id` — the tithe's id, as returned by the list and create endpoints.

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
                    [FromBody] TitheCreateDto dto,
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

                ### One tithe per member per month
                A member has at most one tithe for any `forYear` + `forMonth`. A second one is refused with `409`; to top up, update the existing tithe to the new total instead. A deleted tithe does not count, so a mistaken one can be deleted and recorded again. This is enforced by a unique index in the database as well as by a check before saving, so two requests at the same moment cannot both succeed.

                ### Side effects
                - The tithe is recorded. `GET /tithes/summary` reflects it on next call.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member does not exist in this congregation.
                - `409 CONFLICT` (`errorCode` `TITHE_DUPLICATE`) — the member already has a tithe for that month.
                """
            )
            .Produces<TitheResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] TitheUpdateDto dto,
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
                All fields optional: `amount`, `forYear`, `forMonth`, `paymentMethod`, `collectionDate`, `description`.

                The member cannot be changed: a tithe stays with the member it was recorded for. If it was recorded for the wrong member, delete it and record it again. A `memberId` in the body is ignored.

                ### On success
                Returns `200 OK` with the full updated `TitheResponseDto`.

                ### Side effects
                - If the amount changed, `GET /tithes/summary` recalculates on next call.

                ### Path parameters
                - `id` — the tithe's id, as returned by the list and create endpoints.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no tithe exists with the given ID in this congregation, or the record has been soft-deleted.
                - `409 CONFLICT` (`errorCode` `TITHE_DUPLICATE`) — `forYear`/`forMonth` would move the tithe to a month the member already has a tithe for.
                """
            )
            .Produces<TitheResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

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
                - The member can have a new tithe for that month again.

                ### Path parameters
                - `id` — the tithe's id, as returned by the list and create endpoints.

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
                    [FromQuery] bool? includePrevious,
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
                        includePrevious == true,
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
                - `previousTotalCollected` — the same sum for the stretch of the same length before the window (ending the day before `from`). **Zero unless you send `includePrevious=true`**, which costs one more query, so leave it out when you do not compare.
                - `byMethod` — how the tithes were paid: for each method its `count` and `total`, the method with the most tithes first. Use it for a "how it was paid" split instead of asking once per method.
                - `uniqueTithers` — count of distinct members who gave at least one matching tithe.
                - `mostUsedPaymentMethod` — the payment method with the most matching tithes, as a string. Null when no tithes match.
                - `averagePerMember` — `totalCollected / uniqueTithers`. Zero when there are no tithers.

                ### Filters
                All optional, and the same as on the list endpoint.
                - `memberId` — only this member's tithes.
                - `memberName` — partial, case-insensitive match on the member's name.
                - `paymentMethod` — only tithes paid this way.
                - `year` / `month` — the period the tithe is *for* (not when it was paid).
                - `from` / `to` — only tithes collected inside this date range.
                - `includePrevious` — `true` to also add up the stretch of the same length before the window, into `previousTotalCollected`. Off by default: it costs one more query, so leave it out when you do not compare.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<TitheSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary/monthly",
                async (
                    [AsParameters] TitheMonthlyFilters filters,
                    TitheService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.tithe.monthly_summary"
                    );
                    var result = await service.MonthlySummary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetTitheMonthlySummary")
            .WithSummary("Returns tithes collected per month.")
            .WithDescription(
                """
                Returns one row per calendar month, so the Tithes page can draw a bar chart, a trend line or a donut from a single request.

                Tithes are grouped by the month they are **for** (`forYear` and `forMonth`), not by the day they were collected, so a December tithe given in January counts toward December.

                ### Query parameters
                - `months` — how many months to return, from 1 to 12. Defaults to 12. The rows end with the month of `to`, so `months=6` is the last six months.
                - `to` — any day in the last month to return. Defaults to today, so the last row is this month so far (a part month). Send an earlier date to look back in time: `months=6&to=2026-03-15` returns October 2025 to March 2026.
                - `memberId` — only tithes of this member.
                - `memberName` — only tithes of members whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `paymentMethod` — only tithes paid this way: `Cash`, `Cheque`, `CreditCard`, `MobileMoney` or `BankTransfer`.

                ### Response
                A list of `{ year, month, totalCollected, uniqueTithers, averagePerMember }`. `uniqueTithers` is how many members gave that month and `averagePerMember` is `totalCollected` over `uniqueTithers` (0 when nobody gave). Oldest month first. A month with nothing in it is returned with zeros, so there are no gaps and the list always has exactly `months` rows.

                ### Why it is one endpoint
                This is the only monthly endpoint for tithes: it serves a dashboard card (`months=6`), a full-year chart, or a donut over the months. The shape is always the same list, so the page decides how to draw it.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters, so asking again straight away costs nothing. A change shows up within 2 minutes.

                ### Failure modes
                - `400 BAD_REQUEST` — `months` is not between 1 and 12.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<TitheMonthlyDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
