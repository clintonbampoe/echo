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
                Returns a cursor-paginated list of all organization member assignments scoped to the authenticated congregation. To filter by a specific member or organization use the dedicated sub-endpoints.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetOrganizationMemberById")
            .WithSummary("Returns a single organization member record by ID.")
            .WithDescription(
                """
                Returns the full organization member assignment record for the given ID.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization member record exists with the given ID in this congregation, or it has been soft-deleted.
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
                Returns a cursor-paginated list of all organization assignments for the specified member. Use this to see every organization a member belongs to.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                Returns a cursor-paginated list of all member assignments for the specified organization. Use this to see everyone who belongs to a particular organization.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateOrganizationMember")
            .WithSummary("Adds a member to an organization.")
            .WithDescription(
                """
                Creates an assignment linking a member to an organization. Both the member and the organization must exist within the same congregation. A member can belong to multiple organizations simultaneously.

                On success, returns `201 Created` with the full assignment record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member or organization does not exist in this congregation.
                """
            )
            .Produces<OrganizationMemberResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

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
                Replaces the fields of an existing organization member assignment. Use this to update a member's role within an organization.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization member record exists with the given ID in this congregation, or it has been soft-deleted.
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
                Soft deletes the organization member assignment, effectively removing the member from the organization. The underlying member and organization records are not affected.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization member record exists with the given ID in this congregation, or it has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
