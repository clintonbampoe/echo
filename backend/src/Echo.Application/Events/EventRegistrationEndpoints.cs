using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Events;

public static class EventRegistrationEndpoints
{
    public static IEndpointRouteBuilder MapEventRegistrationEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/event-registrations")
            .WithTags("Event Registrations")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] PaginationRequest pagination,
                    EventRegistrationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_registration.list"
                    );
                    var result = await service.List(
                        context.User.GetCongregationId(),
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListEventRegistrations")
            .WithSummary("Returns a paginated list of event registrations.")
            .WithDescription(
                """
                Returns a cursor-paginated list of every event registration in the caller's congregation.

                ### When to use this
                For administrative views that show all registrations across all events. To see registrations for one event, use `GET /event-registrations/event/{id}`. For one member's registration history, use `GET /event-registrations/member/{id}`.

                ### Ordering
                Records are ordered by registration date. The direction depends on the server-side default — current implementation orders most recent first.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Registration does not imply attendance
                A record here means the member signed up. It does not mean they showed up. To confirm attendance, check `GET /event-attendance` for the same event/member pair.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<EventRegistrationResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    EventRegistrationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_registration.fetch_by_id"
                    );
                    var result = await service.GetById(id, context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("GetEventRegistrationById")
            .WithSummary("Returns a single event registration by ID.")
            .WithDescription(
                """
                Returns one event registration record, scoped to the caller's congregation.

                The response includes the resolved `eventName` and `memberName`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event registration exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<EventRegistrationResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/event/{id:guid}",
                async (
                    Guid id,
                    [AsParameters] PaginationRequest pagination,
                    EventRegistrationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_registration.list_by_event"
                    );
                    var result = await service.ListByEventId(
                        context.User.GetCongregationId(),
                        id,
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListEventRegistrationsByEvent")
            .WithSummary("Returns paginated registrations for a specific event.")
            .WithDescription(
                """
                Returns who has signed up for a specific event, cursor-paginated.

                ### Path parameter
                `id` is the **Event ID**, not the registration ID.

                ### Ordering
                Ordered by registration date, earliest first (sign-up order).

                ### Use case
                Capacity planning, headcount forecasting, pre-event reminder lists. Combine with `GET /event-attendance/event/{id}` to identify members who registered but did not show up.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event exists with the given ID in this congregation.
                """
            )
            .Produces<PagedResponse<EventRegistrationResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/member/{id:guid}",
                async (
                    Guid id,
                    [AsParameters] PaginationRequest pagination,
                    EventRegistrationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_registration.list_by_member"
                    );
                    var result = await service.ListByMemberId(
                        context.User.GetCongregationId(),
                        id,
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListEventRegistrationsByMember")
            .WithSummary("Returns paginated registrations for a specific member.")
            .WithDescription(
                """
                Returns a single member's full event registration history, cursor-paginated.

                ### Path parameter
                `id` is the **Member ID**, not the registration ID.

                ### Ordering
                Ordered by registration date, most recent first.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member exists with the given ID in this congregation.
                """
            )
            .Produces<PagedResponse<EventRegistrationResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    EventRegistrationCreateDto dto,
                    EventRegistrationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_registration.create"
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
            .WithName("CreateEventRegistration")
            .WithSummary("Registers a member for an event.")
            .WithDescription(
                """
                Signs a member up for a specific event.

                ### Request body
                Required fields:
                - `memberId` — the Member being registered.
                - `eventId` — the Event being registered for.
                - `registrationDate` — the date the registration is recorded.

                Both `memberId` and `eventId` must reference records in the caller's congregation.

                ### Registration does not equal attendance
                This endpoint creates a **sign-up**, not a check-in. It does **not** create an Event Attendance record. If the member also showed up, record that separately via `POST /event-attendance`.

                ### On success
                Returns `201 Created` with the full `EventRegistrationResponseDto` and a `Location` header pointing to `GET /event-registrations/{id}`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member or event does not exist in this congregation.
                """
            )
            .Produces<EventRegistrationResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    EventRegistrationUpdateDto dto,
                    EventRegistrationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_registration.update"
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
            .WithName("UpdateEventRegistration")
            .WithSummary("Updates an existing event registration.")
            .WithDescription(
                """
                Updates the registration date on a registration record.

                ### What you can change
                Only `registrationDate`. **`memberId` and `eventId` are immutable** — if you registered the wrong person or the wrong event, delete the registration and create a new one.

                ### On success
                Returns `200 OK` with the full updated `EventRegistrationResponseDto`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `registrationDate` failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event registration exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<EventRegistrationResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    EventRegistrationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_registration.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteEventRegistration")
            .WithSummary("Soft deletes an event registration.")
            .WithDescription(
                """
                Soft-deletes the event registration. The row is retained in the database but excluded from list and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The registration is marked as deleted.
                - **The corresponding attendance record, if any, is not affected.** If the member both registered and attended, deleting the registration leaves the attendance record intact. Attendance is a separate resource.

                ### When to use this
                - The member cancelled their registration.
                - The registration was entered by mistake.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event registration exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
