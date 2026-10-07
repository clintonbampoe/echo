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
                    [AsParameters] EventFilters filters,
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
                Events are ordered by start date. The direction depends on the server-side default — the current implementation orders ascending by start date. There is no client-facing sort parameter.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

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
                    EventCreateDto dto,
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
                    Guid id,
                    EventUpdateDto dto,
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
                - **Existing Event Registrations and Event Attendance for this event are not affected.** They remain in the database and continue to reference the (now deleted) event. When those records are fetched via their own endpoints, `eventName` may resolve to null.
                - **New registrations or attendance cannot reference a deleted event** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.

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
                "/search",
                async (
                    [FromQuery] string q,
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

        return app;
    }
}
