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
                Returns the complete list of attendance types for the authenticated congregation. Attendance types are lightweight lookup entities — no pagination is applied and the full list is always returned.

                Fetch this list to populate type selectors when recording attendance.

                ### Errors
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
                Returns the attendance type record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance type exists with the given ID in this congregation, or it has been soft-deleted.
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
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateAttendanceType")
            .WithSummary("Creates a new attendance type.")
            .WithDescription(
                """
                Creates a new attendance type scoped to the authenticated congregation. Once created, it becomes available for use when recording attendance.

                On success, returns `201 Created` with the full attendance type record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
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
                Replaces the fields of an existing attendance type. Existing attendance records that reference this type retain their assignment after the update.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance type exists with the given ID in this congregation, or it has been soft-deleted.
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
                Marks the attendance type as deleted. The record is retained in the database but excluded from all list, search, and lookup results.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no attendance type exists with the given ID in this congregation, or it has already been soft-deleted.
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
                Performs a trigram-based similarity search against attendance type names using `pg_trgm`. Results are ranked by similarity to the query string `q`. Returns a flat list — no pagination.

                This endpoint is rate-limited. Excessive requests will be rejected.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<AttendanceTypeSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
