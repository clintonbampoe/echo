using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Projects;

public static class ProjectContributionEndpoints
{
    public static IEndpointRouteBuilder MapProjectContributionEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/project-contributions")
            .WithTags("Project Contributions")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] ProjectContributionFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.list"
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
            .WithName("ListProjectContributions")
            .WithSummary("Returns a paginated list of project contributions.")
            .WithDescription(
                """
                Returns a cursor-paginated list of Project Contributions scoped to the caller's congregation.

                ### Filtering
                All filters are optional and combinable:
                - `projectId` — contributions toward one Project.
                - `minAmount` / `maxAmount` — filter by amount range.
                - `paymentMethod` — `Cash`, `Cheque`, `CreditCard`, `MobileMoney`, `BankTransfer`.
                - `from` / `to` — filter by contribution date range.

                **There is no member filter.** Contributions are anonymous — no member is referenced. See the **Project Contributions** tag description for the reasoning.

                ### Ordering
                Contributions are ordered by contribution date, most recent first. Stable across pages.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<ProjectContributionResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetProjectContributionById")
            .WithSummary("Returns a single project contribution by ID.")
            .WithDescription(
                """
                Returns the full Project Contribution record for the given ID, scoped to the caller's congregation.

                The response includes the resolved `projectName` — no follow-up call needed.

                ### Path parameters
                - `id` — the project contribution's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no contribution exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<ProjectContributionResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] ProjectContributionCreateDto dto,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.create"
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
            .WithName("CreateProjectContribution")
            .WithSummary("Records a new project contribution.")
            .WithDescription(
                """
                Records a contribution made toward a Project.

                ### Request body
                Required fields:
                - `projectId` — must reference a Project in the caller's congregation.
                - `amount` — 0.01 to 1,000,000.
                - `dateContributed` — ISO date.
                - `paymentMethod` — `Cash`, `Cheque`, `CreditCard`, `MobileMoney`, `BankTransfer`.
                - `description` — up to 2000 characters. Required even though the schema permits an empty string — the DTO marks this field non-nullable.

                ### Contributions are anonymous
                **There is no member field.** Contributions are not attributed to a Member. This is by design — see the **Project Contributions** tag description. If you need member-attributed giving, use Tithes.

                ### On success
                Returns `201 Created` with the full `ProjectContributionResponseDto` and a `Location` header pointing to `GET /project-contributions/{id}`.

                ### Side effects
                - The contribution is recorded.
                - **`GET /projects/summary` will reflect the new amount in `totalRaised`** on the next call.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced project does not exist in this congregation.
                """
            )
            .Produces<ProjectContributionResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] ProjectContributionUpdateDto dto,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.update"
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
            .WithName("UpdateProjectContribution")
            .WithSummary("Updates an existing project contribution.")
            .WithDescription(
                """
                Updates the supplied fields on an existing Project Contribution. Partial update.

                ### Request body
                All fields optional: `amount`, `dateContributed`, `paymentMethod`, `description`.

                **`projectId` is immutable.** You cannot move a contribution from one project to another. Delete the contribution and create a new one against the correct project.

                ### On success
                Returns `200 OK` with the full updated `ProjectContributionResponseDto`.

                ### Side effects
                - If the amount changed, `GET /projects/summary` recalculates `totalRaised` on next call.

                ### Path parameters
                - `id` — the project contribution's id, as returned by the list and create endpoints.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no contribution exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<ProjectContributionResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteProjectContribution")
            .WithSummary("Soft deletes a project contribution.")
            .WithDescription(
                """
                Soft-deletes the Project Contribution. The row is retained in the database but excluded from list, summary, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The contribution is marked as deleted.
                - **Project summary totals will reflect the deletion.** `GET /projects/summary` and `GET /project-contributions/summary` recalculate from live contributions only.

                ### When to use this
                - The contribution was entered by mistake.
                - The contribution was refunded or reversed and the congregation does not want it in the totals.

                ### Path parameters
                - `id` — the project contribution's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no contribution exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] ProjectContributionFilters filters,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetProjectContributionSummary")
            .WithSummary("Returns aggregate summary metrics for project contributions.")
            .WithDescription(
                """
                Returns aggregated contribution metrics for the caller's congregation, optionally scoped by the same filters as the list endpoint.

                ### Response fields
                - `totalContributed` — sum of amounts across matching contributions.
                - `totalContributions` — count of matching contributions.
                - `averageAmount` — mean contribution size.
                - `mostUsedPaymentMethod` — the payment method with the most contributions, as a string. Null when no contributions match.

                ### Difference from `/projects/summary`
                This endpoint aggregates across **all** contributions unless filtered by `projectId`. `GET /projects/summary` aggregates per project. Use this one when you want congregation-wide contribution stats; use that one when you want per-project funding progress.

                ### Filters
                All optional, and the same as on the list endpoint.
                - `minAmount` / `maxAmount` — only contributions inside this amount range.
                - `paymentMethod` — only contributions paid this way.
                - `from` / `to` — only contributions made inside this date range.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<ProjectContributionSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary/monthly",
                async (
                    [AsParameters] ProjectContributionMonthlyFilters filters,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.monthly_summary"
                    );
                    var result = await service.MonthlySummary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetProjectContributionMonthlySummary")
            .WithSummary("Returns money given to projects per month.")
            .WithDescription(
                """
                Returns one row per calendar month, so the Projects page can chart what has been brought in from a single request. Give `projectId` for one project's own series.

                Contributions are grouped by the month they were given (`dateContributed`).

                ### Query parameters
                - `months` — how many months to return, from 1 to 12. Defaults to 12. The rows end with the month of `to`, so `months=6` is the last six months.
                - `to` — any day in the last month to return. Defaults to today, so the last row is this month so far (a part month). Send an earlier date to look back in time: `months=6&to=2026-03-15` returns October 2025 to March 2026.
                - `projectId` — only contributions to this project.
                - `categoryId` — only contributions to projects of this category.
                - `paymentMethod` — only contributions made this way: `Cash`, `Cheque`, `CreditCard`, `MobileMoney` or `BankTransfer`.

                ### Response
                A list of `{ year, month, totalRaised, contributionCount }`. Oldest month first. A month with nothing in it is returned with zeros, so there are no gaps and the list always has exactly `months` rows.

                ### Why it is one endpoint
                This is the only monthly endpoint for project contributions: it serves a dashboard card (`months=6`), a full-year chart, or a donut over the months. The shape is always the same list, so the page decides how to draw it.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters, so asking again straight away costs nothing. A change shows up within 2 minutes.

                ### Failure modes
                - `400 BAD_REQUEST` — `months` is not between 1 and 12.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<ProjectContributionMonthlyDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
