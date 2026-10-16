using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Attendances;

public static class AttendanceEndpoints
{
    public static IEndpointRouteBuilder MapAttendanceEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/attendance").WithTags("Attendance").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] AttendanceFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    AttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance.list"
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
            .WithName("ListAttendance")
            .WithSummary("Returns a paginated list of attendance records.")
            .WithDescription(
                """
                Returns a cursor-paginated list of attendance records scoped to the caller's congregation.

                ### What is an attendance record
                A record linking a person (Member or Visitor) to a specific Session. Every attendance belongs to exactly one session — the Sunday service, the midweek meeting, the Christmas Eve service.

                ### Filtering
                All filters are optional and combinable:
                - `personId` — records for one specific Member or Visitor.
                - `sessionId` — attendance for one specific session. Common for the "who checked in today" admin view.
                - `sessionLabelId` — attendance across all sessions with a given label.
                - `kind` — filter by the person's kind **at the time of attendance**, not their current kind.
                - `name` — case-insensitive match anywhere in the person's name.
                - `from` / `to` — filter by session date range (inclusive).

                ### About `kind`
                The `kind` filter matches the person's role when the attendance was recorded, not their role now. A visitor who later becomes a member still appears as a Visitor for their pre-conversion attendance. This is deliberate — historical reports should reflect what was true when the record was written.

                ### Ordering
                Attendance records are ordered by session date (most recent first), then by ID, newest first. IDs are time-ordered, so within one session the person checked in last comes first. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<AttendanceResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    AttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetAttendanceById")
            .WithSummary("Returns a single attendance record by ID.")
            .WithDescription(
                """
                Returns one attendance record, scoped to the caller's congregation.

                The response includes the resolved session date, session name, session label name, person name, and the person's kind at the time of attendance — the client does not need to follow up with additional calls.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance record exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<AttendanceResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/bulk",
                async (
                    [FromBody] BulkAttendanceCreateDto dto,
                    AttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance.create_bulk"
                    );
                    var result = await service.CreateBulk(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("BulkCreateAttendance")
            .WithSummary("Marks many people present for one session in a single request.")
            .WithDescription(
                """
                Marks up to 500 people present for one session. Use it for the "Mark present" panel and for catch-up entry instead of one request per person.

                ### Request body
                - `sessionId` — **required.** The session; it must exist in your congregation.
                - `personIds` — **required**, 1 to 500 person ids (members or visitors).
                - `checkedInAt` — optional. Applied to every record created. Defaults to the current time.

                Each person's kind (member or visitor) is recorded as it is at the time, exactly as for a single entry.

                ### On success
                `200 OK` with:
                - `created` — the attendance records that were created, as returned by `POST /attendance`.
                - `skipped` — people who were not marked, each with a `reason`:
                  - `AlreadyPresent` — already marked present for this session. This is not an error here.
                  - `PersonNotFound` — no such person in your congregation (for example a stale id).

                An id sent more than once is counted once. All the new records are saved together, so a failure saves none of them.

                ### Failure modes
                - `400 VALIDATION_ERROR` — no ids, or more than 500.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — the session does not exist.
                - `409 CONFLICT` with `errorCode: "ATTENDANCE_CONFLICT"` — someone marked one of these people present at the very same moment. Nothing was saved; send the request again and they come back as `AlreadyPresent`.
                """
            )
            .Produces<BulkAttendanceResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] AttendanceCreateDto dto,
                    AttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance.create"
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
            .WithName("CreateAttendance")
            .WithSummary("Records a new attendance entry.")
            .WithDescription(
                """
                Records that a person attended a specific session.

                ### Request body
                Required fields:
                - `sessionId` — must reference a Session in the caller's congregation.
                - `personId` — the Member or Visitor attending. Both kinds are accepted.

                Optional fields:
                - `checkedInAt` — the exact moment of check-in. Defaults to the current time, so a person marked by hand gets the time they were marked.

                **The person's kind is not supplied in the request.** The server snapshots it from the person's current `Kind` at the moment of recording. If the person later converts from Visitor to Member, existing attendance records still report them as a Visitor — the record reflects what was true when it was written.

                ### Duplicate attendance
                A person can only be marked present once per session. If an attendance record already exists for this `(sessionId, personId)` pair, the request returns `409 CONFLICT`. Delete the existing record first if you need to re-record.

                ### On success
                Returns `201 Created` with the full `AttendanceResponseDto` and a `Location` header pointing to `GET /attendance/{id}`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced session or person does not exist in this congregation.
                - `409 CONFLICT` — an attendance record already exists for this person and session.
                """
            )
            .Produces<AttendanceResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] AttendanceUpdateDto dto,
                    AttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance.update"
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
            .WithName("UpdateAttendance")
            .WithSummary("Updates an existing attendance record.")
            .WithDescription(
                """
                Updates the check-in time on an attendance record.

                ### What you can change
                Only one field is editable via this endpoint:
                - `checkedInAt`

                **`personId`, `sessionId`, and the recorded person kind are immutable.** If you recorded the wrong person or the wrong session, delete the record and create a new one. The person kind is a historical snapshot — it is not a value that gets corrected.

                ### Request body
                All fields optional. Partial update — omitted fields retain their values.

                ### On success
                Returns `200 OK` with the full updated `AttendanceResponseDto`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — the field failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance record exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<AttendanceResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    AttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteAttendance")
            .WithSummary("Soft deletes an attendance record.")
            .WithDescription(
                """
                Soft-deletes the attendance record. The row is retained in the database but excluded from list, summary, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The record is marked as deleted.
                - **Summary totals reflect the deletion.** `GET /attendance/summary` recalculates from live records only.
                - **Session attendance counts reflect the deletion.** `attendanceCount` on `GET /sessions/{id}` drops by one.
                - The person and the session are not affected — only this one record is removed.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance record exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] AttendanceFilters filters,
                    AttendanceService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetAttendanceSummary")
            .WithSummary("Returns aggregate summary metrics for attendance.")
            .WithDescription(
                """
                Returns aggregated attendance counts for the caller's congregation, optionally scoped by the same filters as the list endpoint.

                ### Response fields
                - `totalPresent` — count of all attendance records matching the filter.
                - `membersPresent` — count where the person was a Member **at the time of attendance**.
                - `visitorsPresent` — count where the person was a Visitor **at the time of attendance**.
                - `firstTimeVisitors` — count of visitors whose earliest visitor attendance falls within the filter range.

                All counts are based on the person's kind as recorded on the attendance row, not their current kind. A visitor who later became a member is still counted as a visitor for the attendances they made before converting.

                Use this endpoint to power reporting dashboards. The filter parameters match the list endpoint exactly, so a summary and its corresponding list are guaranteed to describe the same population.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<AttendanceSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
