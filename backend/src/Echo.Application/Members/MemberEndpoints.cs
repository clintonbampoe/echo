using System.ComponentModel.DataAnnotations;
using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Members;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/members").WithTags("Members").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] MemberFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.list"
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
            .WithName("ListMembers")
            .WithSummary("Returns a paginated list of members.")
            .WithDescription(
                """
                Returns the congregation's members, ordered by name, one page at a time.

                ### Filters
                All optional and combinable.
                - `name` — only members whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `status` — only members with this status: `Active`, `Inactive`, `Archived` or `Transferred`.
                - `gender` — only `Male`, `Female` or `NonBinary`.
                - `region` — only members from this region: `Ahafo`, `Ashanti`, `Bono`, `BonoEast`, `Central`, `Eastern`, `GreaterAccra`, `NorthEast`, `Northern`, `Oti`, `Savannah`, `UpperEast`, `UpperWest`, `Volta`, `Western` or `WesternNorth`.
                - `maritalStatus` — only `Single`, `Married` or `Widowed` members.
                - `organizationId` — only members of this organization (ids come from `GET /organizations`).
                - `from`, `to` — only members who joined between these two days, inclusive (by `joinedDate`). **With no dates, the window is this year.** Members with no join date are kept in every window, so they never disappear from the list.

                ### Ordering
                By name, then by id for members with the same name. The order is fixed, so pages never skip or repeat a row.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### Response
                A page `{ hasMore, next, data, window }`. Only a name, phone number, gender and date of birth are always present: every other field (`residentialAddress`, `city`, `hometown`, `region`, `maritalStatus`, `nextOfKin`, the emergency contact, `baptisedDate`) can be `null`. For the total behind the list (the "12 of 248" footer), call `GET /members/summary` with the same filters and read `totalMembers`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<MemberResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, MemberService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetMemberById")
            .WithSummary("Returns a single member by ID.")
            .WithDescription(
                """
                Returns the full member profile for the given ID, scoped to the caller's congregation. Includes personal details, contact information, next of kin, and membership status.

                ### Path parameters
                - `id` — the member's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<MemberResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] MemberCreateDto dto,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.create"
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
            .WithName("CreateMember")
            .WithSummary("Creates a new member.")
            .WithDescription(
                """
                Adds a member to the congregation. A member needs very little to be created; the rest can be filled in later with `PUT /members/{id}`.

                If the person is already a visitor, use `POST /visitors/{id}/convert` instead: it keeps their history. This endpoint always makes a new person.

                ### Request body
                Required:
                - `firstName`, `lastName` — 1 to 100 characters each.
                - `phoneNumber` — a phone number, up to 20 characters.
                - `dateOfBirth` — a date (`1990-04-02`). Required: it is never guessed.
                - `gender` — `Male`, `Female` or `NonBinary`. Required: it is never guessed.

                Optional (leave out, or send `null`, what you do not know):
                - `otherNames` (up to 100 characters), `emailAddress`.
                - `joinedDate` — the day they joined the church. Without it the member shows up in every date window.
                - `baptisedDate` — the day they were baptised. Without it they are not counted as baptised.
                - `residentialAddress` (up to 255), `city`, `hometown` (up to 100), `gpsAddress` (up to 255).
                - `region` — one of the sixteen regions listed on the list endpoint.
                - `maritalStatus` — `Single`, `Married` or `Widowed`.
                - `nextOfKin`, `emergencyContactName` (up to 100), `emergencyContactPhoneNumber` (a phone number).
                - `status` — `Active` (the default), `Inactive`, `Archived` or `Transferred`.

                ### Response
                `201 Created` with the member and a `Location` header pointing to `GET /members/{id}`.

                A member is not a user: no sign-in account is created. If the person needs to sign in, create a user for them separately.

                ### Failure modes
                - `400 VALIDATION_ERROR` — a required field is missing or a value is malformed; the `errors` object says which.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<MemberResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] MemberUpdateDto dto,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.update"
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
            .WithName("UpdateMember")
            .WithSummary("Updates an existing member.")
            .WithDescription(
                """
                Changes the fields you send; fields you leave out keep their value. This is also how the details left out at creation are filled in later.

                ### Path parameters
                - `id` — the member's id, as returned by the list and create endpoints.

                ### Request body
                All optional; send only what changed. A field cannot be set back to empty: omit it to leave it as it is.
                - `firstName`, `lastName` (1 to 100 characters), `otherNames` (up to 100), `emailAddress`, `phoneNumber`.
                - `dateOfBirth`, `gender` (`Male`, `Female` or `NonBinary`).
                - `joinedDate`, `baptisedDate` — the days they joined and were baptised.
                - `residentialAddress` (up to 255), `city`, `hometown` (up to 100), `gpsAddress` (up to 255).
                - `region`, `maritalStatus` — values as on the list endpoint.
                - `nextOfKin`, `emergencyContactName`, `emergencyContactPhoneNumber`.
                - `status` — `Active`, `Inactive`, `Archived` or `Transferred`.

                ### Response
                `200 OK` with the updated member.

                ### Failure modes
                - `400 VALIDATION_ERROR` — a value is malformed; the `errors` object says which.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member has this id in the congregation, or the member has been deleted.
                """
            )
            .Produces<MemberResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, MemberService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteMember")
            .WithSummary("Soft deletes a member.")
            .WithDescription(
                """
                Soft-deletes the Member **and** the underlying Person record. The rows are retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The Member is marked as deleted.
                - The Person record backing the Member is also soft-deleted.
                - **Records linked to the member are not affected.** Attendance, organization memberships, tithe records, event registrations, and event attendance all remain in the database and continue to reference the deleted member. Queries that join to the member will see it as missing.
                - **User accounts are not affected.** If a User account exists that happens to correspond to this person, it remains active. The two records are not linked.

                ### Reversibility
                Not exposed as reversible through the API.

                ### Path parameters
                - `id` — the member's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchMembers")
            .WithSummary("Searches members by name.")
            .WithDescription(
                """
                Trigram-based similarity search against member names using `pg_trgm`. Searches across full name, first name, and last name — a query of `"Ama Mensah"` matches a member named that, and a query of `"Mensah"` matches any member with that surname.

                ### Query parameter
                - `q` — required. Case-insensitive.

                ### On success
                Returns `200 OK` with a flat array of `MemberSearchResultDto`, ranked by similarity. **No pagination.** Each result includes `id`, `name`, and `phoneNumber` — enough to disambiguate common names without a follow-up call.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy. Debounce the query in the UI.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<MemberSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] MemberFilters filters,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetMemberSummary")
            .WithSummary("Returns aggregate summary metrics for members.")
            .WithDescription(
                """
                Returns the headline numbers for the Members page, for the members the filters pick.

                ### Query parameters
                Same meaning as on the list, so the numbers describe the list the page is showing. `totalMembers` is also the total for a "12 of 248" footer.
                - `name` — only members whose name contains this text (case-insensitive; `%` and `_` are matched literally).
                - `status` — only members with this status: `Active`, `Inactive`, `Archived` or `Transferred`.
                - `gender` — only `Male`, `Female` or `NonBinary`.
                - `region` — only members from this region: `Ahafo`, `Ashanti`, `Bono`, `BonoEast`, `Central`, `Eastern`, `GreaterAccra`, `NorthEast`, `Northern`, `Oti`, `Savannah`, `UpperEast`, `UpperWest`, `Volta`, `Western` or `WesternNorth`.
                - `maritalStatus` — only `Single`, `Married` or `Widowed` members.
                - `organizationId` — only members of this organization (ids come from `GET /organizations`).
                - `from`, `to` — only members who joined between these two days, inclusive (by `joinedDate`). **With no dates, the window is this year.** Members with no join date are kept in every window, so they never disappear from the list.

                ### Response
                - `totalMembers` — how many members match.
                - `activeMembers` — how many of them are `Active`.
                - `maleCount`, `femaleCount` — how many are male and female (members recorded as `Other` are in neither).
                - `averageAge` — their average age in years, from the full date of birth: someone whose birthday has not come yet this year is a year younger.
                - `ageGroups` — always five entries, in this order: `Under 18`, `18–29`, `30–44`, `45–59`, `60+`. They add up to `totalMembers`.
                - `baptisedMembers` — how many have a `baptisedDate`.
                - `joinedInWindow` — members who joined inside the window.
                - `joinedInPreviousWindow` — members who joined in the window before it (as many days, ending the day before `from`). Compare the two to show growth.
                - `joinedThisYear` — members who joined this calendar year so far, whatever the window.
                - `window` — the dates that were applied.

                The three `joined…` figures ignore `from` and `to` as filters (they are about joins, not about which members to list) but still respect `name`, `status`, `gender`, `region`, `maritalStatus` and `organizationId`.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<MemberSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary/monthly",
                async (
                    [AsParameters] MemberMonthlyFilters filters,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.monthly_summary"
                    );
                    var result = await service.MonthlySummary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetMemberMonthlySummary")
            .WithSummary("Returns new members per month.")
            .WithDescription(
                """
                Returns one row per calendar month, so the Members page can chart how the congregation is growing from a single request.

                Members are grouped by the month they joined (`joinedDate`). A member with no join date is not in any month.

                ### Query parameters
                - `months` — how many months to return, from 1 to 12. Defaults to 12. The rows end with the month of `to`, so `months=6` is the last six months.
                - `to` — any day in the last month to return. Defaults to today, so the last row is this month so far (a part month). Send an earlier date to look back in time: `months=6&to=2026-03-15` returns October 2025 to March 2026.
                - `status` — only members with this status (`Active` or `Inactive`).
                - `gender` — only members of this gender: `Male`, `Female` or `NonBinary`.

                ### Response
                A list of `{ year, month, newMembers }`. Oldest month first. A month with nothing in it is returned with zeros, so there are no gaps and the list always has exactly `months` rows.

                ### Why it is one endpoint
                This is the only monthly endpoint for members: it serves a dashboard card (`months=6`), a full-year chart, or a donut over the months. The shape is always the same list, so the page decides how to draw it.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters, so asking again straight away costs nothing. A change shows up within 2 minutes.

                ### Failure modes
                - `400 BAD_REQUEST` — `months` is not between 1 and 12.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<MemberMonthlyDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
