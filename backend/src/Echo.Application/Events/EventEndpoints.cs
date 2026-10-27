using System.ComponentModel.DataAnnotations;
using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Events;

public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/events").WithTags("Events").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] EventListFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    EventService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.list"
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
            .WithName("ListEvents")
            .WithSummary("Returns a paginated list of events.")
            .WithDescription(
                """
                Returns a cursor-paginated list of Events scoped to the caller's congregation.

                ### Filtering
                All filters are optional and combinable:
                - `name` — partial, case-insensitive match against the event name.
                - `organizationId` — events organized by a specific Organization.
                - `organizerId` — events with a specific Member as organizer.
                - `from` / `to` — filter by start-date range (inclusive).

                ### Ordering
                Events are ordered by start date. `order=asc` (the default) lists the oldest first; `order=desc` lists the newest first, which suits a "Past" tab. A page's `next` cursor only continues a list in the direction it was fetched; passing it with the other `order` starts the list again from the first page.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<EventResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, EventService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetEventById")
            .WithSummary("Returns a single event by ID.")
            .WithDescription(
                """
                Returns the full Event record for the given ID, scoped to the caller's congregation.

                The response includes the resolved `organizationName` and `organizerName`, so the client does not need follow-up calls to render the event.

                ### Path parameters
                - `id` — the event's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<EventResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] EventCreateDto dto,
                    EventService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.create"
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
            .WithName("CreateEvent")
            .WithSummary("Creates a new event.")
            .WithDescription(
                """
                Creates a new Event scoped to the caller's congregation.

                ### Request body
                Required fields:
                - `name` — 1 to 100 characters.
                - `organizationId` — **required.** The Organization responsible for the event.
                - `organizerId` — **required.** The Member coordinating the event.
                - `startDate` — required.
                - `endDate` — required.

                Optional fields:
                - `startTime`, `endTime`
                - `location` — up to 255 characters.
                - `capacity` — maximum attendees, 1 to 100,000.
                - `description` — up to 2000 characters.

                **Both `organizationId` and `organizerId` are required despite what the OpenAPI schema may imply.** If either is missing, the service returns `FOREIGN_KEY_NOT_FOUND` rather than a validation error — the FK check runs first.

                ### On success
                Returns `201 Created` with the full `EventResponseDto` and a `Location` header pointing to `GET /events/{id}`.

                ### Side effects
                - A new Event record is created. No registrations or attendance records are created automatically — those are separate resources with their own endpoints.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced organization or organizer does not exist in this congregation.
                """
            )
            .Produces<EventResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] EventUpdateDto dto,
                    EventService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.update"
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
            .WithName("UpdateEvent")
            .WithSummary("Updates an existing event.")
            .WithDescription(
                """
                Updates the supplied fields on an existing Event. Partial update — omitted fields retain their current values.

                ### Request body
                All fields are optional. The DTO allows updating `name`, `startDate`, `endDate`, `startTime`, `endTime`, `location`, `capacity`, `description`, and — unlike most update endpoints — `organizationId` and `organizerId`.

                If either `organizationId` or `organizerId` is supplied, the new value is validated against the congregation's FK constraints. The referenced organization or member must exist in the same congregation.

                ### On success
                Returns `200 OK` with the full updated `EventResponseDto`.

                ### Side effects
                - **Existing registrations and attendance are not affected.** A member who registered for the event under the old organizer remains registered. The organizer field on the event is informational.
                - Changing the date does not invalidate registrations or attendance — the client is responsible for reconciling any scheduling changes.

                ### Path parameters
                - `id` — the event's id, as returned by the list and create endpoints.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event exists with the given ID in this congregation, or the record has been soft-deleted.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced organization or organizer does not exist in this congregation.
                """
            )
            .Produces<EventResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, EventService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteEvent")
            .WithSummary("Soft deletes an event.")
            .WithDescription(
                """
                Soft-deletes the Event. The row is retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The event is marked as deleted.
                - **Existing Event Registrations and Event Attendance for this event are not affected.** They remain in the database and continue to reference the (now deleted) event. When those records are fetched via their own endpoints, `eventName` still returns the deleted event's name.
                - **New registrations or attendance cannot reference a deleted event** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.

                ### Path parameters
                - `id` — the event's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] EventFilters filters,
                    EventService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetEventSummary")
            .WithSummary("Returns aggregate summary metrics for events.")
            .WithDescription(
                """
                Returns the headline numbers for the Events page, for the events the filters pick.

                ### Query parameters
                - `name` — only events whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `organizationId` — only events organized by this organization.
                - `organizerId` — only events organized by this member.
                - `from`, `to` — only events that **start** between these two days, inclusive. Defaults to this year. The response's `window` says which dates were applied.

                ### Response
                - `totalEvents` — events that start inside the window.
                - `upcomingEvents` — events that start today or later, matching `name`, `organizationId` and `organizerId`. It is **not** limited by `from`/`to`, so it also counts events outside the window.
                - `totalRegistered` — registrations for the matching events.
                - `totalAttended` — check-ins for the matching events (members only).
                - `window` — the date range that was applied.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<EventSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/search",
                async (
                    [FromQuery, StringLength(100, MinimumLength = 1)] string q,
                    EventService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchEvents")
            .WithSummary("Searches events by name.")
            .WithDescription(
                """
                Trigram-based similarity search over event names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive. Partial matches supported.

                ### On success
                Returns `200 OK` with a flat array of `EventSearchResultDto`, ranked by similarity. **No pagination.**

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<EventSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary/by-event",
                async (
                    [AsParameters] EventSeriesFilters filters,
                    EventService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.series_summary"
                    );
                    var result = await service.SeriesSummary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetEventSeriesSummary")
            .WithSummary("Returns registrations and check-ins for the newest events.")
            .WithDescription(
                """
                Returns the newest events that match the filters, each with how many people registered and how many were checked in, so the Events page can draw "check-ins per event" (bars, a ranking, a fill-rate gauge) from a single request.

                ### Query parameters
                - `name` — only events whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `organizationId` — only events organized by this organization.
                - `organizerId` — only events organized by this member.
                - `from`, `to` — only events that **start** between these two days, inclusive. Defaults to this year, so the usual call, with no dates, is "the latest events of the year".
                - `limit` — how many events to return, from 1 to 50. Defaults to 12.

                ### Ordering
                The events are always the **newest first**, by start date. The list endpoint's `order` parameter does not exist here: this endpoint is about recent events, and `limit` keeps the newest ones. To rank them (for example "best attended"), sort the returned rows by `attendanceCount` on the client.

                ### Response
                A list of `{ eventId, name, startDate, capacity, registrationCount, attendanceCount }`.
                - `registrationCount` — people registered for the event.
                - `attendanceCount` — people checked in. Check-ins are for members only, so there is no visitor count.
                - `capacity` — the seats the event has, or `null` when it has no limit. The share of seats filled is `attendanceCount / capacity` where `capacity` is set.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters.

                ### Failure modes
                - `400 VALIDATION_ERROR` — `limit` is not between 1 and 50.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<EventSeriesDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary/monthly",
                async (
                    [AsParameters] EventMonthlyFilters filters,
                    EventService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event.monthly_summary"
                    );
                    var result = await service.MonthlySummary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetEventMonthlySummary")
            .WithSummary("Returns events held per month.")
            .WithDescription(
                """
                Returns one row per calendar month, so the Events page can chart how many events were organised, and how well they were attended, from a single request.

                Events are grouped by the month they **start** in, and their registrations and check-ins count toward that month.

                ### Query parameters
                - `months` — how many months to return, from 1 to 12. Defaults to 12. The rows end with the month of `to`, so `months=6` is the last six months.
                - `to` — any day in the last month to return. Defaults to today, so the last row is this month so far (a part month). Send an earlier date to look back in time: `months=6&to=2026-03-15` returns October 2025 to March 2026.
                - `organizationId` — only events organized by this organization.
                - `organizerId` — only events organized by this member.

                ### Response
                A list of `{ year, month, eventCount, registrationCount, attendanceCount }`. Check-ins are for members only. Oldest month first. A month with nothing in it is returned with zeros, so there are no gaps and the list always has exactly `months` rows.

                ### Why it is one endpoint
                This is the only monthly endpoint for events: it serves a dashboard card (`months=6`), a full-year chart, or a donut over the months. The shape is always the same list, so the page decides how to draw it.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters, so asking again straight away costs nothing. A change shows up within 2 minutes.

                ### Failure modes
                - `400 BAD_REQUEST` — `months` is not between 1 and 12.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<EventMonthlyDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
