using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/users").WithTags("Users").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] PaginationRequest pagination,
                    UserService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.list"
                    );
                    var result = await service.List(
                        context.User.GetCongregationId(),
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListUsers")
            .WithSummary("Returns a paginated list of users.")
            .WithDescription(
                """
                Returns a cursor-paginated list of users scoped to the authenticated congregation. Only congregation administrators should have access to this endpoint.

                ### Pagination  
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors  
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<UserResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, UserService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetUserById")
            .WithSummary("Returns a single user by ID.")
            .WithDescription(
                """
                Returns the full user record for the given ID, scoped to the authenticated congregation.

                ### Errors  
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or they have been soft-deleted.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    UserCreateDto dto,
                    UserService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.create"
                    );
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateUser")
            .WithSummary("Creates a new user.")
            .WithDescription(
                """
                Creates a new user account scoped to the authenticated congregation. The email address must be unique within the congregation. After creation, the user will receive a verification email before they can log in.

                On success, returns `201 Created` with the full user record and a `Location` header pointing to the newly created resource.

                ### Errors  
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `409 CONFLICT` — a user with the given email address already exists in this congregation.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UserUpdateDto dto,
                    UserService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.update"
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
            .WithName("UpdateUser")
            .WithSummary("Updates an existing user.")
            .WithDescription(
                """
                Replaces the fields of an existing user record. All updatable fields must be supplied.

                ### Errors  
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or they have been soft-deleted.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, UserService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteUser")
            .WithSummary("Soft deletes a user.")
            .WithDescription(
                """
                Marks the user as deleted. The record is retained in the database but the user can no longer authenticate. This operation is scoped to the congregation — a user deleted here is only removed from this congregation.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or they have already been soft-deleted.
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
                    UserService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchUsers")
            .WithSummary("Searches users by name.")
            .WithDescription(
                """
                Performs a trigram-based similarity search against user names using `pg_trgm`. Results are ranked by similarity to the query string `q`. Returns a flat list — no pagination.

                This endpoint is rate-limited. Excessive requests will be rejected.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<UserSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
