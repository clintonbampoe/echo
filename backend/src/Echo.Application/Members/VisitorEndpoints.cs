using System.ComponentModel.DataAnnotations;
using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Members;

public static class VisitorEndpoints
{
    public static IEndpointRouteBuilder MapVisitorEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/visitors").WithTags("Visitors").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] VisitorFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.list"
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
            .WithName("ListVisitors")
            .WithSummary("Returns a paginated list of visitors.")
            .WithDescription(
                """
                Returns the church's visitors, ordered by name, one page at a time.

                ### Filters
                All optional and combinable.
                - `name` — only visitors whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `gender` — only visitors of this gender: `Male`, `Female` or `NonBinary`.
                - `source` — only visitors who came this way: `WalkIn`, `Invited`, `Online` or `Other`.
                - `invitedByMemberId` — only visitors invited by this member.
                - `converted` — `false` or leave out (the default) for the **current visitors** who have not become members; `true` for the **converted** ones, the historical view.
                - `from`, `to` — a window of dates, inclusive. For current visitors it is the day they were first recorded (`createdAt`); with `converted=true` it is the day they were converted. **With no dates, the window is this year.**

                By default you get the people who are **still visitors** (not converted) who were first recorded this year. A visitor who became a member leaves the default list and appears under `converted=true`.

                ### Ordering
                By name, then by id. The order is fixed, so pages never skip or repeat a row.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### Response
                A page `{ hasMore, next, data, window }`. Each visitor carries `gender`, `address`, `source`, who invited them (`invitedByMemberId`, `invitedByName`), and how often they came (`visitCount`, `lastVisitedAt`). Fields that were not recorded are `null`; a visitor recorded before gender or source was asked has neither.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<VisitorResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetVisitorById")
            .WithSummary("Returns a single visitor by ID.")
            .WithDescription(
                """
                Returns the full Visitor record for the given ID, scoped to the caller's congregation.

                If the visitor has been converted to a Member, the response includes `convertedToMemberPersonId`, `convertedToMemberName`, and `convertedAt`. You can use `convertedToMemberPersonId` to fetch the Member via `GET /members/{id}`.

                ### Path parameters
                - `id` — the visitor's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no visitor exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<VisitorResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] VisitorCreateDto dto,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.create"
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
            .WithName("CreateVisitor")
            .WithSummary("Records a new visitor.")
            .WithDescription(
                """
                Records someone who came for the first time and is not yet a member. A visitor's profile is small by design: the rest (address details, next of kin, emergency contact) is captured when they are converted.

                ### Request body
                Required:
                - `firstName`, `lastName` — 1 to 100 characters each.
                - `gender` — `Male`, `Female` or `NonBinary`. Visitors give their gender, not their age.

                Optional:
                - `phoneNumber` (up to 20 characters), `emailAddress`.
                - `address` — where they live, up to 255 characters.
                - `source` — how they came: `WalkIn`, `Invited`, `Online` or `Other`.
                - `invitedByMemberId` — the member who invited them. It must be a member of this congregation. Naming someone fills `source` in as `Invited`; it cannot be combined with `WalkIn` or `Online`.
                - `notes` — free text, up to 2000 characters: who they are, follow-up reminders.

                ### Response
                `201 Created` with the visitor and a `Location` header pointing to `GET /visitors/{id}`.

                ### Failure modes
                - `400 VALIDATION_ERROR` — a required field is missing, `gender` is not `Male`, `Female` or `NonBinary`, or a value is too long; the `errors` object says which.
                - `400 BAD_REQUEST` — `invitedByMemberId` was given with a `source` of `WalkIn`, `Online` or `Other`.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — `invitedByMemberId` is not a member of this congregation.
                """
            )
            .Produces<VisitorResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] VisitorUpdateDto dto,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.update"
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
            .WithName("UpdateVisitor")
            .WithSummary("Updates an existing visitor.")
            .WithDescription(
                """
                Changes the fields you send; fields you leave out keep their value.

                ### Path parameters
                - `id` — the visitor's id, as returned by the list and create endpoints.

                ### Request body
                All optional; send only what changed.
                - `firstName`, `lastName` (1 to 100 characters), `phoneNumber`, `emailAddress`.
                - `gender` — `Male`, `Female` or `NonBinary`.
                - `address` — up to 255 characters.
                - `source` — `WalkIn`, `Invited`, `Online` or `Other`.
                - `invitedByMemberId` — the member who invited them; the same rules as when creating.
                - `notes` — up to 2000 characters.

                Whether a visitor has been converted cannot be changed here: a conversion is permanent.

                ### Response
                `200 OK` with the updated visitor.

                ### Failure modes
                - `400 VALIDATION_ERROR` — a value is malformed or `gender` is not `Male`, `Female` or `NonBinary`.
                - `400 BAD_REQUEST` — the resulting `source` is `WalkIn`, `Online` or `Other` while someone is named as having invited them.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no visitor has this id in the congregation, or the visitor has been deleted.
                - `404 FOREIGN_KEY_NOT_FOUND` — `invitedByMemberId` is not a member of this congregation.
                """
            )
            .Produces<VisitorResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteVisitor")
            .WithSummary("Soft deletes a visitor.")
            .WithDescription(
                """
                Soft-deletes the Visitor and the underlying Person record. Both are excluded from list, search, and lookup responses but retained in the database.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The Visitor and Person rows are marked as deleted.
                - **Attendance records that reference the visitor are not affected.** If the visitor had been counted in any general attendance records, those records remain and continue to reference the (now deleted) person.
                - If the visitor was previously converted to a Member, the resulting Member record is **not** affected by this deletion. Deleting the visitor does not delete the member.

                ### When to use this
                - Someone was recorded as a visitor by mistake.
                - A duplicate visitor record needs to be removed.

                Not to be confused with conversion. If you want to promote the visitor to a Member, use `POST /visitors/{id}/convert` — do not delete the visitor.

                ### Path parameters
                - `id` — the visitor's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no visitor exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/search",
                async (
                    [FromQuery, StringLength(100, MinimumLength = 1)] string q,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchVisitors")
            .WithSummary("Searches visitors by name.")
            .WithDescription(
                """
                Trigram-based similarity search against visitor names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive.

                ### On success
                Returns `200 OK` with a flat array of `VisitorSearchResultDto`, ranked by similarity. **No pagination.** Each result includes `id`, `name`, and `phoneNumber`.

                ### What is searched
                All visitor records are eligible — converted and unconverted alike. Converted visitors will appear in results. If your UI needs to filter them out, check the `converted` flag via `GET /visitors/{id}`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<VisitorSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/{id:guid}/convert",
                async (
                    [FromRoute] Guid id,
                    [FromBody] MemberCreateDto dto,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.convert"
                    );
                    var result = await service.Convert(
                        context.User.GetCongregationId(),
                        id,
                        dto,
                        context,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ConvertVisitorToMember")
            .WithSummary("Converts a visitor record into a full member.")
            .WithDescription(
                """
                Turns a visitor into a member, keeping the person and their whole history (their visits stay on their record as visits made while a visitor).

                ### Path parameters
                - `id` — the visitor's id, as returned by the list and create endpoints.

                ### Request body
                The same shape as `POST /members`. What is applied is the **member side**:
                - `dateOfBirth`, `gender` — required.
                - `baptisedDate`, `residentialAddress`, `city`, `hometown`, `gpsAddress`, `region`, `maritalStatus`, `nextOfKin`, `emergencyContactName`, `emergencyContactPhoneNumber`, `status` — optional, as for a new member.

                The person's `firstName`, `lastName`, `otherNames`, `emailAddress` and `phoneNumber` stay as they were recorded on the visitor: they must be present for the request to validate, but are not applied. Change them with `PUT /members/{id}` afterwards. `joinedDate` is set to today whatever is sent.

                ### Response
                `201 Created` with the new member and a `Location` header pointing to `GET /members/{id}`. The member has the **same id** as the visitor, so an id the client already holds keeps working.

                The visitor record is kept, marked converted (`convertedAt`, `convertedToMemberPersonId`, `convertedToMemberName`), and moves from the default visitor list to `converted=true`.

                ### Failure modes
                - `400 VALIDATION_ERROR` — a required field is missing or a value is malformed.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no visitor has this id in the congregation, or the visitor has been deleted.
                - `409 CONFLICT` — the visitor was already converted. Read the member with `GET /visitors/{id}` (`convertedToMemberPersonId`) instead.
                """
            )
            .Produces<MemberResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] VisitorFilters filters,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetVisitorSummary")
            .WithSummary("Returns aggregate summary metrics for visitors.")
            .WithDescription(
                """
                Returns the headline numbers for the Visitors page.

                ### Query parameters
                Same meaning as on the list, so the numbers describe the list the page is showing.
                - `name` — only visitors whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `gender` — only visitors of this gender: `Male`, `Female` or `NonBinary`.
                - `source` — only visitors who came this way: `WalkIn`, `Invited`, `Online` or `Other`.
                - `invitedByMemberId` — only visitors invited by this member.
                - `converted` — `false` or leave out (the default) for the **current visitors** who have not become members; `true` for the **converted** ones, the historical view.
                - `from`, `to` — a window of dates, inclusive. For current visitors it is the day they were first recorded (`createdAt`); with `converted=true` it is the day they were converted. **With no dates, the window is this year.**

                ### Response
                - `totalVisitors` — current (or, with `converted=true`, converted) visitors who match: the people the list shows.
                - `newVisitors` — visitors first recorded inside the window, converted or not.
                - `convertedVisitors` — visitors converted to members inside the window.
                - `recurringVisitors` — visitors in the list who came again: they have a visit that was not their first.
                - `invitedVisitors` — visitors in the list whose `source` is `Invited`. Divide by `totalVisitors` for "invited %".
                - `maleVisitors`, `femaleVisitors` — the gender split of the list. Visitors with no gender recorded, or `NonBinary`, are in neither.
                - `window` — the dates that were applied.

                The figures overlap on purpose (a visitor can be new and recurring), so they do not add up to `totalVisitors`.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<VisitorSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
