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
                Returns a cursor-paginated list of all event registrations scoped to the authenticated congregation. To filter by a specific event or member use the dedicated sub-endpoints.

                Registration does not imply attendance — attendance is tracked separately via the Event Attendance resource.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetEventRegistrationById")
            .WithSummary("Returns a single event registration by ID.")
            .WithDescription(
                """
                Returns the full event registration record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event registration exists with the given ID in this congregation, or it has been soft-deleted.
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
                Returns a cursor-paginated list of registrations for the specified event. Use this to see who has signed up for a particular event.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                Returns a cursor-paginated list of event registrations for the specified member. Use this to see all events a member has signed up for.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateEventRegistration")
            .WithSummary("Registers a member for an event.")
            .WithDescription(
                """
                Registers a member for a specific event. Both the member and the event must exist within the same congregation. Registration does not automatically record attendance — use the Event Attendance resource to record actual presence.

                On success, returns `201 Created` with the full registration record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member or event does not exist in this congregation.
                """
            )
            .Produces<EventRegistrationResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

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
                Replaces the fields of an existing event registration. All updatable fields must be supplied.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event registration exists with the given ID in this congregation, or it has been soft-deleted.
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
                Marks the event registration as deleted. The record is retained in the database but excluded from all list and lookup results.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no event registration exists with the given ID in this congregation, or it has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
