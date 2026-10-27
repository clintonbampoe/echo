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
                    Returns the church's gatherings (sessions), most recent first, with who attended each, one page at a time.

                    ### What is a session
                    A specific gathering: the 8am Sunday service on 2026-10-12, the midweek Bible study on 2026-10-15. Attendance records belong to a session, not just to a date, so two services on the same Sunday stay apart.

                    ### Filters
                    All optional and combinable.
                    - `sessionLabelId` — only sessions with this label (for example Sunday service). Ids come from `GET /attendance/labels`.
                    - `status` — only sessions with this status: `Scheduled`, `Completed` or `Cancelled`.
                    - `name` — only sessions whose custom name contains this text (case-insensitive; `%` and `_` are matched literally). A session with no custom name never matches.
                    - `from`, `to` — only sessions held between these two days, inclusive. **With no dates, the window is this week so far** (Sunday to today).

                    ### Ordering
                    By `date`, newest first, then by id for sessions on the same day. The order is fixed, so pages never skip or repeat a row.

                    ### Paging
                    - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                    - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                    Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                    ### Response
                    A page `{ hasMore, next, data, window }`. Each session carries who was there, so the list needs no follow-up calls:
                    - `attendanceCount` — everyone marked present.
                    - `membersPresent`, `visitorsPresent` — the split of that total, by what each person was when they attended.
                    - `firstTimeVisitors` — visitors for whom this was their first-ever visit; part of `visitorsPresent`.
                    - `location` — where it was held, if set (for example "Main hall").

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
                    Returns one session with its label, category, creator and attendance (total, members, visitors and first-time visitors), so no follow-up call is needed.

                    ### Path parameters
                    - `id` — the session's id, as returned by the list and create endpoints.

                    ### Failure modes
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    - `404 NOT_FOUND` — no session has this id in the congregation, or it has been deleted.
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
                    Records a gathering that took place or is planned.

                    ### Request body
                    Required:
                    - `sessionLabelId` — the label of the session (for example Sunday service). Must exist in the congregation (`GET /attendance/labels`).
                    - `date` — the day it begins. For an overnight vigil, the day it starts.

                    Optional:
                    - `name` — a custom name that replaces the label in displays, for example "Christmas Eve Service". Up to 200 characters.
                    - `topic` — the sermon or teaching topic, up to 500 characters.
                    - `location` — where it is held, for example "Main hall". Up to 200 characters.
                    - `notes` — free text, up to 2000 characters.
                    - `startsAt`, `endsAt` — the times it runs, as clock times (`09:30:00`).

                    ### Response
                    `201 Created` with the session and a `Location` header pointing to `GET /attendance/sessions/{id}`. A new session starts as `Scheduled` with no attendance.

                    ### Failure modes
                    - `400 VALIDATION_ERROR` — a field is missing or too long; the `errors` object says which.
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    - `404 FOREIGN_KEY_NOT_FOUND` — `sessionLabelId` is not a label of this congregation.
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
                    Changes the fields you send; fields you leave out keep their value.

                    ### Path parameters
                    - `id` — the session's id, as returned by the list and create endpoints.

                    ### Request body
                    All optional; send only what changed.
                    - `name` — custom name, up to 200 characters.
                    - `topic` — up to 500 characters.
                    - `location` — where it is held, up to 200 characters.
                    - `notes` — up to 2000 characters.
                    - `startsAt`, `endsAt` — the times it runs.
                    - `status` — `Scheduled`, `Completed` or `Cancelled`. Mark a session `Completed` after it happens and `Cancelled` if it does not; only completed sessions count in the averages.

                    `date` and `sessionLabelId` cannot be changed: a different date or label is a different session, so delete this one and create another. That keeps the attendance attached to what actually happened.

                    ### Response
                    `200 OK` with the updated session, including its current attendance.

                    ### Failure modes
                    - `400 VALIDATION_ERROR` — a field is too long; the `errors` object says which.
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    - `404 NOT_FOUND` — no session has this id in the congregation, or it has been deleted.
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
                    Removes a session that was created by mistake.

                    ### Path parameters
                    - `id` — the session's id, as returned by the list and create endpoints.

                    ### What it does
                    - The session disappears from lists.
                    - **Its attendance records are deleted with it.** They no longer appear in attendance lists or counts, including the summaries and the first-time and recurring visitor counts.
                    - A visitor whose first visit was at this session has their next-earliest visit marked as the first one.

                    ### When to use it
                    Only for sessions that should never have existed. A session that was planned but did not happen should be set to `Cancelled` instead (`PUT`): that keeps the history and leaves it out of the averages.

                    ### Response
                    `204 No Content`. Totals update within the 2-minute cache.

                    ### Failure modes
                    - `401 UNAUTHORIZED` — missing or invalid bearer token.
                    - `404 NOT_FOUND` — no session has this id in the congregation, or it was already deleted.
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
                    Returns the headline numbers for the sessions the filters pick.

                    ### Query parameters
                    Same meaning as on the list, so the numbers describe the list the page is showing.
                    - `sessionLabelId` — only sessions with this label (for example Sunday service). Ids come from `GET /attendance/labels`.
                    - `status` — only sessions with this status: `Scheduled`, `Completed` or `Cancelled`.
                    - `name` — only sessions whose custom name contains this text (case-insensitive; `%` and `_` are matched literally). A session with no custom name never matches.
                    - `from`, `to` — only sessions held between these two days, inclusive. **With no dates, the window is this week so far** (Sunday to today).

                    ### Response
                    - `totalSessions` — how many sessions match.
                    - `completedSessions` — how many of them are `Completed`.
                    - `cancelledSessions` — how many are `Cancelled`.
                    - `totalAttendance` — everyone marked present at the matching sessions.
                    - `averageAttendance` — how many people a completed session drew: the attendance at completed sessions over the number of completed sessions. `0` when none was held.
                    - `window` — the dates that were applied.

                    ### Caching
                    The answer is cached for 2 minutes per congregation and per set of parameters.

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
