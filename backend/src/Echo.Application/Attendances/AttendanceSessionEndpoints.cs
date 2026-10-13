using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Attendances;

public static class AttendanceSessionEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapSessionEndpoints(ApplicationInstrumentation instrumentation)
        {
            var group = app.MapGroup("/attendance/sessions")
                .WithTags("Attendance Sessions")
                .RequireAuthorization();

            group
                .MapGet(
                    "/",
                    async (
                        [AsParameters] SessionFilters filters,
                        [AsParameters] PaginationRequest pagination,
                        SessionService service,
                        HttpContext context,
                        CancellationToken ct
                    ) =>
                    {
                        using var span = EndpointInstrumentation.StartSpan(
                            context,
                            instrumentation,
                            "endpoint.session.list"
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
                .WithName("ListSessions")
                .WithSummary("Returns a paginated list of attendance sessions.")
                .WithDescription(
                    """
                    Returns a cursor-paginated list of Attendance Sessions scoped to the caller's congregation.

                    ### What is a session
                    A session is a specific gathering — the 8am Sunday Service on 2026-10-12, the midweek Bible Study on 2026-10-15. Attendance records reference a session, not just a date and type. This is what makes it possible to distinguish two services on the same Sunday, or a Christmas Eve service that coincides with the regular Sunday service.

                    ### Filtering
                    All filters are optional and combinable:
                    - `sessionLabelId` — sessions with one specific label.
                    - `status` — `Scheduled`, `Completed`, or `Cancelled`.
                    - `name` — substring match against the session's custom name (case-insensitive).
                    - `from` / `to` — filter by session date range (inclusive).

                    ### Ordering
                    Sessions are ordered by date, most recent first, then by ID as a tiebreaker. Stable across pages.

                    ### Response includes attendance count
                    Each session in the response carries an `attendanceCount` — the number of attendance records for that session. Populated in the same request; no follow-up call needed.

                    ### Pagination
                    Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                    ### Failure modes
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    """
                )
                .Produces<PagedResponse<SessionResponseDto>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group
                .MapGet(
                    "/{id:guid}",
                    async (
                        [FromRoute] Guid id,
                        SessionService service,
                        HttpContext context,
                        CancellationToken ct
                    ) =>
                    {
                        using var span = EndpointInstrumentation.StartSpan(
                            context,
                            instrumentation,
                            "endpoint.session.fetch_by_id"
                        );
                        var result = await service.GetById(
                            context.User.GetCongregationId(),
                            id,
                            ct
                        );
                        return result.ToResult();
                    }
                )
                .WithName("GetSessionById")
                .WithSummary("Returns a single attendance session by ID.")
                .WithDescription(
                    """
                    Returns one session, scoped to the caller's congregation.

                    The response includes the resolved label name, category, creator name (if set), and the current attendance count — no follow-up calls needed.

                    ### Failure modes
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    - `404 NOT_FOUND` — no session exists with the given ID in this congregation, or the record has been soft-deleted.
                    """
                )
                .Produces<SessionResponseDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group
                .MapPost(
                    "/",
                    async (
                        [FromBody] SessionCreateDto dto,
                        SessionService service,
                        HttpContext context,
                        CancellationToken ct
                    ) =>
                    {
                        using var span = EndpointInstrumentation.StartSpan(
                            context,
                            instrumentation,
                            "endpoint.session.create"
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
                .WithName("CreateSession")
                .WithSummary("Creates a new attendance session.")
                .WithDescription(
                    """
                    Records a specific gathering that occurred (or is scheduled to occur).

                    ### Request body
                    Required fields:
                    - `sessionLabelId` — must reference a Session Label in the caller's congregation.
                    - `date` — the date the session begins. For overnight sessions (vigils), this is the start date, not the end date.

                    Optional fields:
                    - `name` — a custom display name overriding the label. E.g. a session labeled "Service" with `name = "Christmas Eve Service"`.
                    - `topic` — the sermon or teaching topic, up to 500 characters.
                    - `notes` — free-form, up to 2000 characters.
                    - `startsAt` / `endsAt` — the wall-clock times the session runs.

                    ### On success
                    Returns `201 Created` with the full `SessionResponseDto` and a `Location` header pointing to `GET /attendance/sessions/{id}`.

                    New sessions start with `status = Scheduled` and `attendanceCount = 0`.

                    ### Failure modes
                    - `400 BAD_REQUEST` — the request body is malformed.
                    - `400 VALIDATION_ERROR` — one or more fields failed validation.
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    - `404 FOREIGN_KEY_NOT_FOUND` — the referenced session label does not exist in this congregation.
                    """
                )
                .Produces<SessionResponseDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group
                .MapPut(
                    "/{id:guid}",
                    async (
                        [FromRoute] Guid id,
                        [FromBody] SessionUpdateDto dto,
                        SessionService service,
                        HttpContext context,
                        CancellationToken ct
                    ) =>
                    {
                        using var span = EndpointInstrumentation.StartSpan(
                            context,
                            instrumentation,
                            "endpoint.session.update"
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
                .WithName("UpdateSession")
                .WithSummary("Updates an existing attendance session.")
                .WithDescription(
                    """
                    Updates the supplied fields on an existing Session. Partial update.

                    ### Editable fields
                    `name`, `topic`, `notes`, `startsAt`, `endsAt`, `status`.

                    ### Not editable
                    `date` and `sessionLabelId` are immutable. Changing either effectively means it's a different session — delete this one and create a new one. This preserves the attendance records attached to this session.

                    ### Status transitions
                    - `Scheduled` → `Completed` — use after the session happens.
                    - `Scheduled` → `Cancelled` — use when the session doesn't occur.
                    - `Completed` and `Cancelled` are terminal; there is no enforcement, but moving back to `Scheduled` would be unusual.

                    ### On success
                    Returns `200 OK` with the full updated `SessionResponseDto`, including the current attendance count.

                    ### Failure modes
                    - `400 BAD_REQUEST` — the request body is malformed.
                    - `400 VALIDATION_ERROR` — one or more fields failed validation.
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    - `404 NOT_FOUND` — no session exists with the given ID in this congregation, or the record has been soft-deleted.
                    """
                )
                .Produces<SessionResponseDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group
                .MapDelete(
                    "/{id:guid}",
                    async (
                        [FromRoute] Guid id,
                        SessionService service,
                        HttpContext context,
                        CancellationToken ct
                    ) =>
                    {
                        using var span = EndpointInstrumentation.StartSpan(
                            context,
                            instrumentation,
                            "endpoint.session.delete"
                        );
                        var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                        return result.ToResult();
                    }
                )
                .WithName("DeleteSession")
                .WithSummary("Soft deletes an attendance session.")
                .WithDescription(
                    """
                    Soft-deletes the session. The row is retained in the database but excluded from list and lookup responses.

                    ### On success
                    Returns `204 No Content` with no body.

                    ### Side effects
                    - The session is marked as deleted.
                    - **Attendance records are not deleted.** They remain in the database with a foreign key to the (now deleted) session. Whether they surface in attendance list responses depends on the query — the current attendance repository filters by `DeletedAt` on the attendance row, not the session's. Confirm your query expectations before relying on this.
                    - **Summary totals will reflect the deletion.** Session summaries recalculate from live records only.

                    ### When to use this
                    Only for sessions that were created in error. If a session was planned and cancelled, set `status = Cancelled` instead — that preserves the record of the session happening (or not happening) and excludes it from active reports.

                    ### Failure modes
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    - `404 NOT_FOUND` — no session exists with the given ID in this congregation, or the record has already been soft-deleted.
                    """
                )
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group
                .MapGet(
                    "/summary",
                    async (
                        [AsParameters] SessionFilters filters,
                        SessionService service,
                        HttpContext context,
                        CancellationToken ct
                    ) =>
                    {
                        using var span = EndpointInstrumentation.StartSpan(
                            context,
                            instrumentation,
                            "endpoint.session.summary"
                        );
                        var result = await service.Summary(
                            context.User.GetCongregationId(),
                            filters,
                            ct
                        );
                        return result.ToResult();
                    }
                )
                .WithName("GetSessionSummary")
                .WithSummary("Returns aggregate summary metrics for attendance sessions.")
                .WithDescription(
                    """
                    Returns aggregated session metrics for the caller's congregation, optionally scoped by the same filters as the list endpoint.

                    ### Response fields
                    - `totalSessions` — count of sessions matching the filter.
                    - `completedSessions` — count where `status = Completed`.
                    - `cancelledSessions` — count where `status = Cancelled`.
                    - `totalAttendance` — sum of attendance counts across matching sessions.
                    - `averageAttendance` — `totalAttendance / totalSessions`. Zero when no sessions match.

                    Use this endpoint to power reporting dashboards. The filter parameters match the list endpoint exactly, so a summary and its corresponding list are guaranteed to describe the same population.

                    ### Failure modes
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    """
                )
                .Produces<SessionSummaryDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            return app;
        }
    }
}
