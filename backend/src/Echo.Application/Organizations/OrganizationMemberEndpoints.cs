using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Organizations;

public static class OrganizationMemberEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationMemberEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/organization-members")
            .WithTags("Organization Members")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] OrganizationMemberFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    OrganizationMemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization_member.list"
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
            .WithName("ListOrganizationMembers")
            .WithSummary("Returns a paginated list of organization members.")
            .WithDescription(
                """
                Returns a cursor-paginated list of every member-organization assignment in the caller's congregation.

                ### When to use this
                For administrative views that show all assignments across all organizations. To see assignments for one member, use `GET /organization-members/member/{id}`. For one organization's roster, use `GET /organization-members/organization/{id}`.

                ### Filtering
                - `role` — `Member`, `Secretary`, or `Leader`. Filter to assignments with a specific role.

                ### Ordering
                Records are ordered by creation date, most recent first. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<OrganizationMemberResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    OrganizationMemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization_member.fetch_by_id"
                    );
                    var result = await service.GetById(id, context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("GetOrganizationMemberById")
            .WithSummary("Returns a single organization member record by ID.")
            .WithDescription(
                """
                Returns one member-organization assignment, scoped to the caller's congregation.

                The response includes the resolved `memberName` and `organizationName`, so no follow-up calls are needed.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no assignment exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<OrganizationMemberResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/member/{id:guid}",
                async (
                    Guid id,
                    [AsParameters] OrganizationMemberFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    OrganizationMemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization_member.list_by_member"
                    );
                    var result = await service.ListByMemberId(
                        context.User.GetCongregationId(),
                        id,
                        filters,
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListOrganizationMembersByMember")
            .WithSummary("Returns paginated organization memberships for a specific member.")
            .WithDescription(
                """
                Returns every organization a given member belongs to, cursor-paginated.

                ### Path parameter
                `id` is the **Member ID**, not the assignment ID.

                ### Filtering
                - `role` — filter to assignments with a specific role.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member exists with the given ID in this congregation.
                """
            )
            .Produces<PagedResponse<OrganizationMemberResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/organization/{id:guid}",
                async (
                    Guid id,
                    [AsParameters] OrganizationMemberFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    OrganizationMemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization_member.list_by_organization"
                    );
                    var result = await service.ListByOrganizationId(
                        context.User.GetCongregationId(),
                        id,
                        filters,
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListOrganizationMembersByOrganization")
            .WithSummary("Returns paginated members for a specific organization.")
            .WithDescription(
                """
                Returns the roster of a given organization, cursor-paginated.

                ### Path parameter
                `id` is the **Organization ID**, not the assignment ID.

                ### Filtering
                - `role` — filter to members with a specific role within the organization.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization exists with the given ID in this congregation.
                """
            )
            .Produces<PagedResponse<OrganizationMemberResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    OrganizationMemberCreateDto dto,
                    OrganizationMemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization_member.create"
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
            .WithName("CreateOrganizationMember")
            .WithSummary("Adds a member to an organization.")
            .WithDescription(
                """
                Creates an assignment linking a Member to an Organization.

                ### Request body
                - `memberId` — required. Must reference a Member in the caller's congregation.
                - `organizationId` — required. Must reference an Organization in the caller's congregation.
                - `role` — required. `Member`, `Secretary`, or `Leader`. This is the member's function **within this organization** — not the platform-wide `UserRole`.
                - `joinedAt` — required. The date the member joined the organization.

                ### On success
                Returns `201 Created` with the full `OrganizationMemberResponseDto` and a `Location` header pointing to `GET /organization-members/{id}`.

                ### A member can belong to multiple organizations
                There is no uniqueness constraint on the `memberId` alone. The same member can be assigned to many organizations simultaneously, each with its own role and join date.

                **The endpoint does not prevent duplicate assignments to the same organization.** Sending the same `(memberId, organizationId)` pair twice creates two records. Guard against this in the client by checking the member's existing assignments via `GET /organization-members/member/{id}` before creating.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member or organization does not exist in this congregation.
                """
            )
            .Produces<OrganizationMemberResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    OrganizationMemberUpdateDto dto,
                    OrganizationMemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization_member.update"
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
            .WithName("UpdateOrganizationMember")
            .WithSummary("Updates an organization member record.")
            .WithDescription(
                """
                Updates a member's role or join date within an organization.

                ### What you can change
                - `role` — the member's function within this organization.
                - `joinedAt` — the recorded join date.

                **`memberId` and `organizationId` are immutable.** If you assigned the wrong member or the wrong organization, delete the assignment and create a new one.

                ### On success
                Returns `200 OK` with the full updated `OrganizationMemberResponseDto`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no assignment exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<OrganizationMemberResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    OrganizationMemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization_member.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteOrganizationMember")
            .WithSummary("Removes a member from an organization.")
            .WithDescription(
                """
                Soft-deletes the assignment, effectively removing the member from the organization.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The assignment is marked as deleted.
                - **The Member record is not affected.** The person remains a Member of the congregation — they are just no longer a member of this organization.
                - **The Organization record is not affected.** The organization still exists and still has its other members.
                - If the member belongs to multiple organizations, the other assignments are untouched.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no assignment exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
