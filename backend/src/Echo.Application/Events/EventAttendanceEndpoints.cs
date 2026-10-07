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
                Returns a cursor-paginated list of all event attendance records scoped to the authenticated congregation. To filter by a specific event or member use the dedicated sub-endpoints.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                Returns the full event attendance record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event attendance record exists with the given ID in this congregation, or it has been soft-deleted.
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
                Returns a cursor-paginated list of attendance records for the specified event. Use this to see who attended a particular event.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                Returns a cursor-paginated list of event attendance records for the specified member. Use this to see a member's full event attendance history.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateEventAttendance")
            .WithSummary("Records attendance for an event.")
            .WithDescription(
                """
                Records that a member attended a specific event. Both the member and the event must exist within the same congregation. A member may register for an event without attending — registrations and attendance are tracked separately.

                On success, returns `201 Created` with the full attendance record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member or event does not exist in this congregation.
                """
            )
            .Produces<EventAttendanceResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

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
                Replaces the fields of an existing event attendance record. All updatable fields must be supplied.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event attendance record exists with the given ID in this congregation, or it has been soft-deleted.
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
                Marks the event attendance record as deleted. The record is retained in the database but excluded from all list and lookup results.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event attendance record exists with the given ID in this congregation, or it has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
