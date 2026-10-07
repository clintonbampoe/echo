using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Events;

public static class EventAttendanceEndpoints
{
    public static IEndpointRouteBuilder MapEventAttendanceEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/event-attendance")
            .WithTags("Event Attendance")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] PaginationRequest pagination,
                    EventAttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_attendance.list"
                    );
                    var result = await service.List(
                        context.User.GetCongregationId(),
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListEventAttendance")
            .WithSummary("Returns a paginated list of event attendance records.")
            .WithDescription(
                """
                Returns a cursor-paginated list of every event attendance record in the caller's congregation.

                ### When to use this
                For administrative views that show all attendance across all events — "who attended what, most recently". To see attendance for one event, use `GET /event-attendance/event/{id}`. For one member's history, use `GET /event-attendance/member/{id}`.

                ### Ordering
                Records are ordered by check-in time, most recent first. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<EventAttendanceResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    EventAttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_attendance.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetEventAttendanceById")
            .WithSummary("Returns a single event attendance record by ID.")
            .WithDescription(
                """
                Returns one event attendance record, scoped to the caller's congregation.

                The response includes the resolved `eventName` and `memberName`, so no follow-up calls are needed to render the record.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event attendance record exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<EventAttendanceResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/event/{id:guid}",
                async (
                    Guid id,
                    [AsParameters] PaginationRequest pagination,
                    EventAttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_attendance.list_by_event"
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
            .WithName("ListEventAttendanceByEvent")
            .WithSummary("Returns paginated attendance records for a specific event.")
            .WithDescription(
                """
                Returns who showed up to a specific event, cursor-paginated.

                ### Path parameter
                `id` is the **Event ID**, not the attendance record ID.

                ### Ordering
                Ordered by check-in time, earliest first (chronological arrival order).

                ### What this does not include
                This returns only members who **attended**. Members who registered but did not show up do **not** appear here — query `GET /event-registrations/event/{id}` and cross-reference to find the "registered but not attended" gap.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event exists with the given ID in this congregation.
                """
            )
            .Produces<PagedResponse<EventAttendanceResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/member/{id:guid}",
                async (
                    Guid id,
                    [AsParameters] PaginationRequest pagination,
                    EventAttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_attendance.list_by_member"
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
            .WithName("ListEventAttendanceByMember")
            .WithSummary("Returns paginated attendance records for a specific member.")
            .WithDescription(
                """
                Returns a single member's full event attendance history, cursor-paginated.

                ### Path parameter
                `id` is the **Member ID**, not the attendance record ID.

                ### Ordering
                Ordered by check-in time, most recent first.

                ### Use case
                Show a member's engagement over time — how many events they've attended, most recently, etc. Combine with `GET /event-registrations/member/{id}` to see registrations versus actual attendance.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member exists with the given ID in this congregation.
                """
            )
            .Produces<PagedResponse<EventAttendanceResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    EventAttendanceCreateDto dto,
                    EventAttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_attendance.create"
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
            .WithName("CreateEventAttendance")
            .WithSummary("Records attendance for an event.")
            .WithDescription(
                """
                Records that a member attended a specific event.

                ### Request body
                Required fields:
                - `memberId` — the Member who attended.
                - `eventId` — the Event attended.
                - `checkInTime` — the time of arrival.

                Both `memberId` and `eventId` must reference records in the caller's congregation.

                ### Registration is not required
                A member does **not** have to be registered for an event to be marked as attended. If a walk-in shows up, record the attendance directly. Echo does not enforce a registration-before-attendance rule.

                Conversely, **creating an attendance record does not create a registration**. If you also want the registration recorded, you must create both.

                ### On success
                Returns `201 Created` with the full `EventAttendanceResponseDto` and a `Location` header pointing to `GET /event-attendance/{id}`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member or event does not exist in this congregation.
                """
            )
            .Produces<EventAttendanceResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    EventAttendanceUpdateDto dto,
                    EventAttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_attendance.update"
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
            .WithName("UpdateEventAttendance")
            .WithSummary("Updates an existing event attendance record.")
            .WithDescription(
                """
                Updates the check-in time on an event attendance record.

                ### What you can change
                Only `checkInTime`. **`memberId` and `eventId` are immutable** — if you recorded the wrong person or the wrong event, delete the record and create a new one.

                ### On success
                Returns `200 OK` with the full updated `EventAttendanceResponseDto`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `checkInTime` failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event attendance record exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<EventAttendanceResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    EventAttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.event_attendance.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteEventAttendance")
            .WithSummary("Soft deletes an event attendance record.")
            .WithDescription(
                """
                Soft-deletes the event attendance record. The row is retained in the database but excluded from list and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The record is marked as deleted.
                - Any event summary derived from attendance is recalculated on next query — the deletion is reflected in `GET /events/summary` counts.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event attendance record exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
