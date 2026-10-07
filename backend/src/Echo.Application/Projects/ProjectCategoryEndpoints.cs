using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Projects;

public static class ProjectCategoryEndpoints
{
    public static IEndpointRouteBuilder MapProjectCategoryEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/project-categories")
            .WithTags("Project Categories")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (ProjectCategoryService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.list"
                    );
                    var result = await service.List(context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("ListProjectCategories")
            .WithSummary("Returns all project categories for the congregation.")
            .WithDescription(
                """
                Returns the complete list of Project Categories for the caller's congregation.

                ### No pagination
                Plain array, not a paged envelope. Project Categories are lightweight lookup entities bounded in number.

                Call this to populate the category selector when creating or updating Projects.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<ProjectCategoryResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:int}",
                async (
                    int id,
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetProjectCategoryById")
            .WithSummary("Returns a single project category by ID.")
            .WithDescription(
                """
                Returns the Project Category record for the given ID, scoped to the caller's congregation.

                ### ID is an integer
                Project Category IDs are 32-bit integers, not UUIDs.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project category exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<ProjectCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    ProjectCategoryCreateDto dto,
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.create"
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
            .WithName("CreateProjectCategory")
            .WithSummary("Creates a new project category.")
            .WithDescription(
                """
                Creates a new Project Category — for example Construction, Outreach, or Equipment. Scoped to the caller's congregation.

                ### Request body
                - `name` — required. 1 to 100 characters.

                ### On success
                Returns `201 Created` with the full `ProjectCategoryResponseDto` and a `Location` header pointing to `GET /project-categories/{id}`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `name` failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<ProjectCategoryResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:int}",
                async (
                    int id,
                    ProjectCategoryUpdateDto dto,
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.update"
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
            .WithName("UpdateProjectCategory")
            .WithSummary("Updates an existing project category.")
            .WithDescription(
                """
                Updates the name of a Project Category. Partial update.

                ### Side effects
                - **Existing Projects assigned to this category are not affected.** They will display the new name via `categoryName` on next fetch.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `name` failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project category exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<ProjectCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:int}",
                async (
                    int id,
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteProjectCategory")
            .WithSummary("Soft deletes a project category.")
            .WithDescription(
                """
                Soft-deletes the Project Category. The row is retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The category is marked as deleted.
                - **Existing Projects assigned to this category are not affected.** They keep their `categoryId` pointing at the deleted category. `categoryName` may resolve to null on fetch.
                - **New projects cannot reference a deleted category** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project category exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchProjectCategories")
            .WithSummary("Searches project categories by name.")
            .WithDescription(
                """
                Trigram-based similarity search over project category names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive.

                ### On success
                Returns `200 OK` with a flat array of `ProjectCategorySearchResultDto`. **No pagination.**

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<ProjectCategorySearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
