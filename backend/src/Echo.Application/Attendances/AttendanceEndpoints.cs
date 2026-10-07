using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
                Returns a cursor-paginated list of general attendance records scoped to the caller's congregation.

                ### What is a general attendance record
                A record linking a **person** (Member or Visitor) to an Attendance Type on a specific date. This is not the same as Event Attendance — see the **Attendance** tag description for the distinction.

                ### Filtering
                All filters are optional and combinable:
                - `attendanceTypeId` — records of one specific type.
                - `kind` — `Member` or `Visitor`. Filter to just members or just visitors.
                - `from` / `to` — filter by attendance date range (inclusive).

                ### Ordering
                Attendance records are ordered by date (most recent first), then by ID as a tiebreaker. Stable across pages.

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
                    Guid id,
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

                The response includes the resolved `attendanceTypeName`, `personName`, and `personKind` — the client does not need to follow up with additional calls to render the record.

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
                "/",
                async (
                    AttendanceCreateDto dto,
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
                Records that a person attended a service or activity on a specific date.

                ### Request body
                Required fields:
                - `attendanceTypeId` — must reference an Attendance Type in the same congregation.
                - `personId` — the Member or Visitor attending. Both kinds are accepted.
                - `date` — the date of attendance.

                Optional fields:
                - `checkInTime` — the time of arrival, if known.
                - `notes` — free-form, up to 2000 characters.

                **The `personKind` is not supplied in the request.** The server derives it from the referenced person's current `Kind`. If the person later converts from Visitor to Member, existing attendance records keep the kind they were recorded with.

                ### On success
                Returns `201 Created` with the full `AttendanceResponseDto` and a `Location` header pointing to `GET /attendance/{id}`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced person or attendance type does not exist in this congregation.
                """
            )
            .Produces<AttendanceResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    AttendanceUpdateDto dto,
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
                Updates the check-in time or notes on an attendance record.

                ### What you can change
                Only two fields are editable via this endpoint:
                - `checkInTime`
                - `notes`

                **`personId`, `attendanceTypeId`, and `date` are immutable.** If you recorded the wrong person or the wrong date, delete the record and create a new one.

                ### Request body
                All fields optional. Partial update — omitted fields retain their values.

                ### On success
                Returns `200 OK` with the full updated `AttendanceResponseDto`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
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
                    Guid id,
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
                - **Summary totals will reflect the deletion.** `GET /attendance/summary` recalculates from live records only.
                - The person and the attendance type are not affected — only this one attendance record is removed.

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
                - `membersPresent` — count where the person was a Member.
                - `visitorsPresent` — count where the person was a Visitor.
                - `firstTimeVisitors` — count of visitors appearing in attendance for the first time within the filter range.

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
