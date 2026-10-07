using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Attendances;

public static class AttendanceTypeEndpoints
{
    public static IEndpointRouteBuilder MapAttendanceTypeEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/attendance-types")
            .WithTags("Attendance Types")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (AttendanceTypeService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance_type.list"
                    );
                    var result = await service.List(context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("ListAttendanceTypes")
            .WithSummary("Returns all attendance types for the congregation.")
            .WithDescription(
                """
                Returns the complete list of attendance types for the caller's congregation.

                ### No pagination
                The response is a plain array, not a paged envelope. Attendance types are lightweight lookup entities bounded in number, so the full list is always returned.

                Call this to populate the type selector when recording attendance.

                ### Ordering
                Types are returned in a stable server-defined order. Do not rely on alphabetical ordering — the sort is not exposed as a parameter.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<AttendanceTypeResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:int}",
                async (
                    int id,
                    AttendanceTypeService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance_type.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetAttendanceTypeById")
            .WithSummary("Returns a single attendance type by ID.")
            .WithDescription(
                """
                Returns the attendance type record for the given ID, scoped to the caller's congregation.

                ### ID is an integer
                Attendance Type IDs are 32-bit integers, not UUIDs. This is deliberate — lookup entities in Echo use integer keys for compactness.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance type exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<AttendanceTypeResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    AttendanceTypeCreateDto dto,
                    AttendanceTypeService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance_type.create"
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
            .WithName("CreateAttendanceType")
            .WithSummary("Creates a new attendance type.")
            .WithDescription(
                """
                Creates a new attendance type — for example "Sunday Service", "Midweek Service", or "Prayer Meeting". Scoped to the caller's congregation.

                ### Request body
                - `name` — required. Minimum 1 character, maximum 100. Should be unique within the congregation for practical purposes, though this is not enforced as a hard constraint.

                ### On success
                Returns `201 Created` with the full `AttendanceTypeResponseDto` and a `Location` header pointing to `GET /attendance-types/{id}`.

                ### Side effects
                - The type becomes immediately available for use when creating Attendance records.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `name` failed validation (missing, empty, or longer than 100 characters).
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<AttendanceTypeResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:int}",
                async (
                    int id,
                    AttendanceTypeUpdateDto dto,
                    AttendanceTypeService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance_type.update"
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
            .WithName("UpdateAttendanceType")
            .WithSummary("Updates an existing attendance type.")
            .WithDescription(
                """
                Updates an attendance type's name. Partial update — only fields present in the request are changed.

                ### Side effects
                - **Existing attendance records that reference this type are not affected** and continue to work. They will now display the updated name in their `attendanceTypeName` field.
                - Records created before the rename are indistinguishable from records created after — there is no versioning of the type name on historical records.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `name` failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance type exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<AttendanceTypeResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:int}",
                async (
                    int id,
                    AttendanceTypeService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance_type.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteAttendanceType")
            .WithSummary("Soft deletes an attendance type.")
            .WithDescription(
                """
                Soft-deletes the attendance type. The row is retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The type is marked as deleted.
                - **Existing attendance records that reference this type are not affected.** They remain in the database with a foreign key to the (now deleted) type. In list responses, `attendanceTypeName` may resolve as null or as the stale name depending on the join, since the type lookup no longer finds the record.
                - **New attendance records cannot reference a deleted type** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.

                ### When to use this
                Only when a type is genuinely obsolete. If you just want to stop using a type for new records, consider leaving it in place — the reference integrity cost of deletion is higher than the readability cost of a stale type in a dropdown.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance type exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                    AttendanceTypeService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.attendance_type.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchAttendanceTypes")
            .WithSummary("Searches attendance types by name.")
            .WithDescription(
                """
                Trigram-based similarity search over attendance type names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive. Partial matches supported.

                ### On success
                Returns `200 OK` with a flat array of `AttendanceTypeSearchResultDto`, ranked by similarity. **No pagination.**

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<AttendanceTypeSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
