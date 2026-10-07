using Asp.Versioning.ApiExplorer;
using Echo.Shared.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

internal sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = "Echo API",
                    Version = description.ApiVersion.ToString(),
                    Description = $"""
                    Echo is a multi-tenant congregation management platform for managing members, attendance, finances, events, and organizational structure.

                    ## Authentication
                    All endpoints require a valid RS256 JWT Bearer token except Auth endpoints. Obtain a token via `POST /api/v1/auth/sessions/login` and pass it as `Authorization: Bearer <token>` on all subsequent requests. Access tokens are short-lived — use `POST /api/v1/auth/sessions/refresh` to obtain a new pair without re-authenticating.

                    ## Congregation Scoping
                    Every request is automatically scoped to the authenticated user's congregation via claims embedded in the JWT. There is no need to pass a congregation identifier — it is enforced server-side and cannot be overridden.

                    ## Pagination
                    List endpoints use cursor-based pagination. Responses include `hasMore` (boolean) and `next` (opaque cursor string). Pass `next` as the `cursor` query parameter on the subsequent request to fetch the next page. When `hasMore` is false, `next` is null and no further pages exist. Default page size is 24, maximum is 24.

                    ## Search
                    Search endpoints use trigram-based similarity matching (`pg_trgm`) and accept a `q` query parameter. Results are ranked by similarity. Search endpoints are rate-limited.

                    ## Soft Deletes
                    All delete operations are soft deletes — records are marked as deleted but not removed from the database. Soft-deleted records are excluded from all list, search, and lookup results automatically.

                    ## Error Responses
                    All error responses conform to RFC 7807 Problem Details and include a machine-readable `errorCode` extension field for client-side conditional handling.

                    {ErrorCatalog.ToMarkdownTable()}
                    """,
                }
            );
        }

        options.DocumentFilter<TagDescriptionDocumentFilter>();
    }
}

internal sealed class TagDescriptionDocumentFilter : IDocumentFilter
{
    private static readonly string[] _tagOrder =
    [
        // Auth
        "Sessions",
        "Passwords",
        "Invitations",
        "Verifications",
        "Registrations",
        // People
        "Users",
        "Members",
        "Visitors",
        // Attendance
        "Attendance",
        "Attendance Types",
        // Events
        "Events",
        "Event Attendance",
        "Event Registrations",
        // Organizations
        "Organizations",
        "Organization Members",
        // Assets
        "Assets",
        "Asset Categories",
        // Projects
        "Projects",
        "Project Categories",
        "Project Contributions",
        // Finances
        "Tithes",
        "Transactions",
        "Transaction Categories",
        // Infrastructure
        "Health",
    ];

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var ranks = new Dictionary<string, int>(_tagOrder.Length);
        for (var i = 0; i < _tagOrder.Length; i++)
        {
            ranks[_tagOrder[i]] = i;
        }

        var comparer = Comparer<OpenApiTag>.Create(
            (a, b) =>
            {
                var rankA = ranks.GetValueOrDefault(a.Name ?? string.Empty, int.MaxValue);
                var rankB = ranks.GetValueOrDefault(b.Name ?? string.Empty, int.MaxValue);
                var rankCompare = rankA.CompareTo(rankB);
                return rankCompare != 0
                    ? rankCompare
                    : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            }
        );

        swaggerDoc.Tags = new SortedSet<OpenApiTag>(comparer)
        {
            new()
            {
                Name = "Sessions",
                Description = """
                    Manages the authentication token lifecycle. Echo issues two tokens at login:

                    - **Access token** — short-lived (currently 15 minutes). Sent as `Authorization: Bearer <token>` on every authenticated request. Carries the user's identity and congregation claims. This is how the server scopes every request to a congregation — the client never sends a congregation identifier, and never can.
                    - **Refresh token** — long-lived (currently 30 days). Used only to obtain a new token pair. Never sent as a bearer token.

                    A *session* is a refresh token. Logging in creates one. Revoking a refresh token ends that one session. Logging out of all sessions ends every session for the user.

                    ### Refresh rotation
                    Refresh tokens are **single-use**. Every successful call to `POST /auth/sessions/refresh` returns a new pair and invalidates the old refresh token. Reusing a spent refresh token returns `401 INVALID_TOKEN` — the client should treat this as a signal of token theft and force re-login.

                    ### Login requirements
                    The account's email must be **verified** before login succeeds.

                    - Wrong email or password → `401 INVALID_CREDENTIALS`.
                    - Correct credentials but unverified email → `403 EMAIL_NOT_VERIFIED`. Send the user into the verification flow, not back to the login form.

                    ### Revocation semantics
                    Access tokens are **not blacklisted** — a deliberate trade-off to avoid the operational cost of a token blocklist. Consequences the client must plan for:

                    - After a successful `revoke` or `logout`, the current access token **keeps working until it expires**, at most ~15 minutes.
                    - The client should discard the access token locally on revoke/logout and refresh on the next request rather than relying on the server to reject it.
                    - This is *why* access tokens are short-lived. Do not increase their lifetime without reintroducing a blocklist.

                    ### Authentication
                    None of these endpoints require a bearer token. They exist to create or tear down a session, so the caller proves possession of the refresh token (or supplies the account email) rather than presenting an access token. See the **Invitations** tag for the only authenticated Auth endpoint.

                    ### Errors
                    `INVALID_CREDENTIALS`, `INVALID_TOKEN`, and `EMAIL_NOT_VERIFIED` are all covered in the error catalog at the top of this document.
                    """,
            },
            new()
            {
                Name = "Passwords",
                Description = """
                    Password reset for users who cannot sign in. Two-step flow.

                    ### Step 1 — `POST /auth/passwords/forgot`
                    If the email is registered, a reset link is sent to it. **The response is identical whether or not the address exists.** This is intentional, to prevent account enumeration. Do not branch UI messaging on the response body — always show the same "if that email is registered, you'll receive a link" confirmation.

                    ### Step 2 — `POST /auth/passwords/reset`
                    The user opens the link from the email; the client posts `email`, `token`, and `newPassword`. The token is **single-use and expires** — invalid, expired, or already-spent tokens return `400 INVALID_TOKEN`. The new password must satisfy the server's password policy (minimum 8 characters); violations return `400 VALIDATION_ERROR`.

                    ### Authentication
                    None required. These endpoints exist precisely because the user has no session.

                    ### Errors
                    See the error catalog for `INVALID_TOKEN`, `VALIDATION_ERROR`, and rate-limit responses.
                    """,
            },
            new()
            {
                Name = "Invitations",
                Description = """
                    The **only authenticated tag under Auth**, and the only one that requires a role.

                    An invitation is a token an administrator mints for a new user. It encodes:

                    - the congregation it belongs to,
                    - the role the invited user will receive on registration (`UserRole` — one of `Admin`, `Accountant`, `Clerk`, `Member`),
                    - an expiry.

                    The caller supplies the role and a requested `expiryDays`. **The server caps the expiry at 30 days** regardless of what's requested — requesting more will not extend it.

                    ### Who can call this
                    An authenticated user with the `Admin` role in their congregation. Any other role receives `403`. The invitation is always scoped to the caller's congregation — an admin cannot invite into another tenant.

                    ### How the invitation is redeemed
                    This endpoint only *mints* the token. The invited user redeems it through `POST /auth/registrations/member`, passing the token alongside their new user details. Until redemption, the invitation is inert — no user record exists yet.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR` (bad role or expiry), `UNAUTHORIZED`, and the 403 response.
                    """,
            },
            new()
            {
                Name = "Verifications",
                Description = """
                    Email verification. Confirms the user controls the address on their account. **Required before login** — unverified accounts are rejected at `POST /auth/sessions/login` with `403 EMAIL_NOT_VERIFIED`.

                    ### Step 1 — `POST /auth/verifications/account`
                    If the email is registered, sends a verification link. **The response is identical whether or not the address exists** — intentional, to prevent account enumeration. Do not branch UI messaging on the response body.

                    ### Step 2 — `POST /auth/verifications/verify-email`
                    The user opens the link from the email; the client submits the `token` from the query string. On success the account's `verifiedAt` is populated and the user can sign in. The token is **single-use and expires** — invalid or spent tokens return `400 INVALID_TOKEN`.

                    ### Authentication
                    None required. These endpoints are used before a session exists.

                    ### Errors
                    See the error catalog for `INVALID_TOKEN`, `VALIDATION_ERROR`, and rate-limit responses.
                    """,
            },
            new()
            {
                Name = "Registrations",
                Description = """
                    Account creation. Two distinct flows, both unauthenticated.

                    ### `POST /auth/registrations/congregation` — new tenant onboarding
                    Creates a **congregation and its first administrator** in a single call, from a `congregationDto` and a `userDto`. This is the entry point for a brand-new customer. The admin account it creates is subject to email verification before it can log in via `POST /auth/sessions/login`.

                    Use this only when the congregation does not yet exist. To add additional users to an existing congregation, use the authenticated `POST /users` endpoint.

                    ### `POST /auth/registrations/member` — join an existing congregation
                    Creates a user account in an existing congregation by redeeming an **invitation token**. The body carries the token and a `userInfo` (`UserCreateDto`). The role comes from the invitation, not the request.

                    This is the redemption endpoint for the flow started by `POST /auth/invitations`.

                    ### Authentication
                    None required. By definition, neither flow has a session yet.

                    ### Lifecycle
                    `register → verify email → log in`. The account is not usable for login until verification completes.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `CONFLICT` (email or congregation already exists), and rate-limit responses.
                    """,
            },
            new()
            {
                Name = "Users",
                Description = """
                    **Users are platform accounts** — the credentials someone uses to sign in to Echo. A User is distinct from a Member: a User has a login, a role, and a congregation; a Member is a person in the congregation's roster. Some congregations may have Users who are not Members (e.g. an administrator) and Members who are not Users.

                    A User's role determines what they can do across the platform. The roles are:

                    - `Admin` — full control of the congregation.
                    - `Accountant` — financial workflows.
                    - `Clerk` — day-to-day data entry.
                    - `Member` — the lowest-privilege authenticated role.

                    Users are always scoped to a single congregation — an account cannot span tenants.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation, though the list endpoint is intended for administrators.

                    ### Lifecycle
                    Created directly via `POST /users` or via the invitation flow. On creation the account is **unverified** — `verifiedAt` is null and login is blocked until the user completes email verification. Update replaces `emailAddress`, `password`, and `role`. Delete is a soft delete — the account is retained but blocked from authentication.

                    ### Dependencies
                    A User references a congregation. The account is the anchor for sessions, invitations, and role-based authorization.

                    ### Search and pagination
                    `GET /users` is cursor-paginated. `GET /users/search` is a flat, rate-limited trigram search. See the top of this document for pagination and search conventions.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `CONFLICT`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Members",
                Description = """
                    **Members are the core people record in Echo.** A Member is a fully registered individual in the congregation with a complete profile: personal details, contact information, next of kin, and membership status.

                    Members are distinct from Visitors. A Visitor is a lightweight, pre-membership record. A Visitor becomes a Member through `POST /visitors/{id}/convert`. The conversion is one-way.

                    ### Why it exists
                    Members are the anchor for most of the platform's data: general attendance records, organization memberships, tithe records, event registrations, and event attendance all reference a Member. If you're building reporting or relationship features, this is the entity you'll join against.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create directly via `POST /members`, or indirectly by converting a Visitor. Update replaces the full record — all updatable fields must be supplied. Delete is a soft delete: the Member disappears from lists, searches, and lookups, but their attendance, organization memberships, and financial records are preserved.

                    ### Key enums
                    - `status` — `Active`, `Inactive`, `Archived`, `Transferred`.
                    - `gender` — `Male`, `Female`, `Other`.
                    - `maritalStatus` — `Single`, `Married`, `Widowed`.
                    - `region` — one of the sixteen Ghana regions (Ashanti, GreaterAccra, …).

                    ### Search, filters, and pagination
                    `GET /members` is cursor-paginated and supports filters by name, status, gender, region, marital status, and join date range. `GET /members/search` is a flat, rate-limited trigram search over first name, last name, and full name. `GET /members/summary` returns `totalMembers`, `activeMembers`, `maleCount`, `femaleCount`, and `averageAge` for the same filter set.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Visitors",
                Description = """
                    **Visitors are pre-membership people records.** A Visitor captures basic contact context for someone who has attended congregation activities but is not yet a full Member.

                    Visitors exist so the congregation can track and follow up on newcomers without creating a full Member profile for each one. Visitor records are intentionally lighter than Member records — next of kin, membership status, and the rest of the Member profile are captured at conversion time.

                    ### Conversion flow
                    `POST /visitors/{id}/convert` promotes a Visitor to a Member. The request body is a full `MemberCreateDto` — it supplies everything the Member profile requires that the Visitor did not have. On success:

                    - A new Member record is created.
                    - The Visitor record is retained for history but **marked as converted**. `convertedToMemberPersonId`, `convertedToMemberName`, and `convertedAt` are populated on the Visitor.
                    - Converted Visitors are excluded from active lists, searches, and lookups.
                    - A Visitor can only be converted once. Attempting to convert an already-converted or soft-deleted Visitor returns `404 NOT_FOUND`.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create → optionally convert to Member, or soft-delete. Update replaces the full record. Delete is a soft delete.

                    ### Search, filters, and pagination
                    `GET /visitors` is cursor-paginated and supports filters by name, converted flag, and date range. `GET /visitors/search` is a flat, rate-limited trigram search. `GET /visitors/summary` returns `totalVisitors`, `newVisitors`, `recurringVisitors`, and `convertedVisitors`.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Attendance",
                Description = """
                    **General attendance tracking for congregation services and activities.** Each record links a **person** to an Attendance Type on a specific date, with an optional check-in time and notes.

                    ### Who counts as a "person"
                    Attendance covers **both Members and Visitors**. The record carries a `personId` and a `personKind` of `Member` or `Visitor`. This matters — newcomers are typically tracked as Visitors before they become Members, and the congregation still wants their service attendance counted. The API does not convert one to the other automatically.

                    ### How this differs from Event Attendance
                    This tag tracks **general** attendance — services, midweek meetings, prayer gatherings. It is not tied to any scheduled Event. If you're recording presence at a specific event (conference, outreach), use **Event Attendance** instead. The two resources are separate and neither feeds the other.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create requires a `personId` (Member or Visitor), an `attendanceTypeId`, and a `date`. Both references must exist in the same congregation. Update is limited — only `checkInTime` and `notes` are editable. Delete is a soft delete.

                    ### Dependencies
                    - **Person** — a Member or Visitor in the same congregation. A missing or cross-tenant reference returns `404 FOREIGN_KEY_NOT_FOUND`.
                    - **Attendance Type** — must exist in the same congregation. Same error.

                    ### Filters and pagination
                    `GET /attendance` is cursor-paginated and supports filters by attendance type, `kind` (`Member` or `Visitor`), and date range. `GET /attendance/summary` returns `totalPresent`, `membersPresent`, `visitorsPresent`, and `firstTimeVisitors` for the same filter set — the split by kind is directly in the response.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Attendance Types",
                Description = """
                    **Attendance Types are congregation-defined categories of service or activity** — for example Sunday Service, Midweek Service, or Prayer Meeting.

                    Every Attendance record must reference an Attendance Type. Types are how the congregation classifies its gatherings for reporting.

                    ### Lookup entity — no pagination
                    `GET /attendance-types` returns the **full list** with no pagination. Types are lightweight and bounded in number, so paging is unnecessary. Call this to populate the type selector when recording attendance.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create a type before creating any Attendance record that references it. Update replaces the full record. Delete is a soft delete — the type disappears from lists and lookups, but **existing Attendance records that reference it are not affected** and retain their classification.

                    ### Dependencies
                    Nothing. This is a leaf lookup entity within a congregation.

                    ### Search
                    `GET /attendance-types/search` is a flat, rate-limited trigram search over type names.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Events",
                Description = """
                    **Events represent scheduled congregation activities** — conferences, outreaches, special services, and similar one-off or recurring gatherings. An Event has a name, a date range, optional times, a location, and an optional capacity.

                    ### Relationships
                    An Event may optionally be linked to:

                    - an **Organization** (`organizationId`) — e.g. events run by a specific ministry,
                    - an **Organizer** (`organizerId`) — the person coordinating it.

                    Both are optional at create time. Only `name` is required.

                    ### The registration / attendance split
                    Registering for an Event and attending an Event are tracked as **two separate resources** (Event Registrations and Event Attendance). The two are not linked:

                    - A member can register and not show up.
                    - A member can show up without registering.
                    - Reports usually need both numbers, and the gap between them.

                    Recording a registration does **not** create an attendance record, and vice versa.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create an Event → members register and/or attend. Updating an Event does not affect existing registrations or attendance. Deleting an Event is a soft delete — the record is retained but excluded from lists and lookups. Existing registrations and attendance for that event remain in the database.

                    ### Search, filters, and pagination
                    `GET /events` is cursor-paginated and supports filters by name, organization, organizer, and date range. `GET /events/search` is a flat, rate-limited trigram search over event names.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Event Attendance",
                Description = """
                    **Event Attendance records who actually showed up to an Event.** Each record links a Member to an Event, representing one person's presence, with an optional check-in time.

                    This is **not** the same as Event Registrations. A registration is a sign-up; attendance is proof of presence. A member may register and not attend, or attend without registering. Echo does not auto-convert a registration into attendance — you record them independently.

                    ### Why it exists
                    Accurate headcount per event, per-member attendance history, and the delta between "signed up" and "showed up" all come from this resource.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### How it's queried
                    - `GET /event-attendance` — every attendance record in the congregation, cursor-paginated.
                    - `GET /event-attendance/event/{id}` — attendance for one event.
                    - `GET /event-attendance/member/{id}` — one member's full attendance history.

                    ### Lifecycle
                    Create links a Member and an Event, both of which must already exist in the same congregation. Update is limited — only `checkInTime` is editable. Delete is a soft delete.

                    ### Dependencies
                    - **Event** — must exist in the same congregation.
                    - **Member** — must exist in the same congregation.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Event Registrations",
                Description = """
                    **Event Registrations capture sign-ups for Events.** Each record links a Member to an Event, representing an intent to attend.

                    Registration does **not** imply attendance. Attendance is tracked separately under Event Attendance. Recording a registration will not create an attendance record, and vice versa. If your UI needs to show "registered but not attended", you'll compare the two resources.

                    ### Why it exists
                    Registration supports capacity planning, headcount forecasting, and reminder workflows before an event. Attendance supports after-the-fact reporting.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### How it's queried
                    - `GET /event-registrations` — every registration in the congregation, cursor-paginated.
                    - `GET /event-registrations/event/{id}` — registrations for one event.
                    - `GET /event-registrations/member/{id}` — one member's full registration history.

                    ### Lifecycle
                    Create links a Member and an Event, both of which must already exist in the same congregation. Update is limited — only `registrationDate` is editable. Delete is a soft delete.

                    ### Dependencies
                    - **Event** — must exist in the same congregation.
                    - **Member** — must exist in the same congregation.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Organizations",
                Description = """
                    **Organizations are internal congregation groups** — departments, ministries, choirs, committees. A congregation can have many Organizations, and a Member can belong to more than one at the same time.

                    Organizations are the container. The link between a Member and an Organization is a separate resource (Organization Members) that also carries the member's role within that Organization. This two-level model lets you change a member's function in an org without touching the Organization itself.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create an Organization → assign members via Organization Members. Updating an Organization does not affect existing assignments. Deleting an Organization is a soft delete — the record is excluded from lists and lookups, but existing member assignments remain in the database.

                    ### Dependencies
                    None for the Organization itself. Organization Members and (optionally) Events depend on it.

                    ### Search, filters, and pagination
                    `GET /organizations` is cursor-paginated. `GET /organizations/search` is a flat, rate-limited trigram search. `GET /organizations/summary` returns `totalOrganizations`, `totalMembers`, `averageMembersPerOrganization`, and `largestOrganization`.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Organization Members",
                Description = """
                    **Organization Members are the join records between a Member and an Organization.** Each record captures the member, the organization, the member's role *within that organization*, and the date they joined.

                    This is a distinct resource rather than a field on Member or Organization because the relationship itself carries data (role, join date) and because a member may belong to multiple organizations with different roles in each.

                    ### Roles within an organization
                    The role is a `MemberRole`, one of `Member`, `Secretary`, or `Leader`. **This is not the same as `UserRole`** — `MemberRole` describes a person's function inside a specific org, while `UserRole` governs what they can do in the platform.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### How it's queried
                    - `GET /organization-members` — every assignment in the congregation, cursor-paginated, filterable by role.
                    - `GET /organization-members/member/{id}` — every organization a given member belongs to.
                    - `GET /organization-members/organization/{id}` — every member of a given organization.

                    ### Lifecycle
                    Create links a Member and an Organization, both of which must already exist in the same congregation. Update replaces the record — this is how you change a member's role within an organization. Delete is a soft delete: the member is effectively removed from the organization, but the Member and Organization records themselves are untouched.

                    ### Dependencies
                    - **Member** — must exist in the same congregation.
                    - **Organization** — must exist in the same congregation.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Assets",
                Description = """
                    **Assets are the congregation's physical and non-physical resources** — equipment, property, vehicles, furniture, and similar inventory. Each Asset belongs to an Asset Category and carries a purchase cost and a current value, which together give you depreciation tracking.

                    ### Why it exists
                    Asset records support financial reporting (what does the congregation own, and what is it worth today) and operational tracking (what do we have, where is it, what condition is it in).

                    ### Status lifecycle
                    Every Asset has an `AssetStatus`:

                    - `Active` — in service and available.
                    - `InUse` — currently deployed for a specific purpose.
                    - `InStorage` — held but not deployed.
                    - `UnderMaintenance` — temporarily out of service for repair.
                    - `Liquidated` — disposed of or written off.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create an Asset → optionally update, soft-delete. Update replaces the full record — all updatable fields must be supplied. Deleting an Asset is a soft delete: the record is retained but excluded from lists, searches, and lookups.

                    ### Dependencies
                    Every Asset must reference an **Asset Category** in the same congregation. Creating or updating an Asset with a missing or cross-tenant category ID returns `404 FOREIGN_KEY_NOT_FOUND`.

                    ### Search, filters, and pagination
                    `GET /assets` is cursor-paginated and supports filters by status, category, name, and purchase date range. `GET /assets/search` is a flat, rate-limited trigram search. `GET /assets/summary` returns `totalAssets`, `totalCurrentValue`, `totalPurchaseCost`, and `totalDepreciation`.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Asset Categories",
                Description = """
                    **Asset Categories are congregation-defined classifications for Assets** — for example Equipment, Furniture, Vehicles.

                    Every Asset must belong to exactly one Asset Category. Categories are how the congregation groups and filters its asset inventory.

                    ### Lookup entity — no pagination
                    `GET /asset-categories` returns the **full list** with no pagination. Categories are lightweight and bounded in number. Call this to populate the category selector when creating or updating an Asset.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create a category before creating any Asset that references it. Update replaces the full record — **existing Assets assigned to that category retain their assignment**. Delete is a soft delete: the category disappears from lists and lookups, but existing Assets that reference it are not affected.

                    ### Dependencies
                    Nothing. This is a leaf lookup entity within a congregation.

                    ### Search
                    `GET /asset-categories/search` is a flat, rate-limited trigram search over category names.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Projects",
                Description = """
                    **Projects are congregation fundraising or development initiatives** — building projects, outreach campaigns, equipment drives. A Project has a funding target, a timeline, a category, and a manager.

                    The money raised toward a Project is tracked as a separate resource (Project Contributions) because each contribution is a distinct event with its own amount, date, and payment method. Project totals are computed by aggregating contributions.

                    ### Who manages a project
                    A Project carries a `managerId` (a Member) and returns `managerName` in responses. The manager is the person responsible for the project's progress — it is not an access-control concept.

                    ### Status
                    `ProjectStatus` is one of `Planning`, `OnTrack`, `AtRisk`, `Complete`, `Missed`. `AtRisk` projects are surfaced in the summary as `atRiskCount`.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create a Project → record contributions against it. Updating a Project (including changing its target or timeline) does not affect existing contributions. Deleting a Project is a soft delete — the record is excluded from lists and lookups, but existing contributions remain in the database.

                    ### Dependencies
                    Every Project must reference a **Project Category** in the same congregation. Creating or updating a Project with a missing or cross-tenant category ID returns `404 FOREIGN_KEY_NOT_FOUND`.

                    ### Search, filters, and pagination
                    `GET /projects` is cursor-paginated and supports filters by name, status, category, and date range. `GET /projects/search` is a flat, rate-limited trigram search. `GET /projects/summary` returns `totalProjects`, `totalRaised`, `totalTarget`, and `atRiskCount`.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Project Categories",
                Description = """
                    **Project Categories are congregation-defined classifications for Projects** — for example Construction, Outreach, Equipment.

                    Every Project must belong to exactly one Project Category. Categories are how the congregation groups and filters its fundraising initiatives.

                    ### Lookup entity — no pagination
                    `GET /project-categories` returns the **full list** with no pagination. Categories are lightweight and bounded in number. Call this to populate the category selector when creating or updating a Project.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create a category before creating any Project that references it. Update replaces the full record — **existing Projects assigned to that category retain their assignment**. Delete is a soft delete: the category disappears from lists and lookups, but existing Projects that reference it are not affected.

                    ### Dependencies
                    Nothing. This is a leaf lookup entity within a congregation.

                    ### Search
                    `GET /project-categories/search` is a flat, rate-limited trigram search over category names.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Project Contributions",
                Description = """
                    **Project Contributions record contributions made toward a Project.** Each record captures an amount, a contribution date, a payment method, and a free-text description against a specific Project. Contributions can be financial or in-kind.

                    ### Contributions are anonymous
                    Contributions are **not linked to a Member**. This is deliberate. Congregations receive project giving from people outside their roster — visitors, external donors, anonymous givers — and want to record the money without creating a Member record for every giver.

                    If you need giving that *is* attributed to a member, use **Tithes**. Project Contributions are for project-specific giving where the giver is either unknown or intentionally not recorded.

                    ### Why it exists
                    Contributions are the source of truth for "how much has been raised for this project." The Project holds the target; contributions hold the progress. `GET /projects/summary` derives `totalRaised` from these records.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create references a Project, which must already exist in the same congregation. Update replaces the full record. Delete is a soft delete — the record is excluded from lists and summary totals will reflect the deletion.

                    ### Dependencies
                    - **Project** — must exist in the same congregation. A missing or cross-tenant project returns `404 FOREIGN_KEY_NOT_FOUND`.

                    ### Filters and pagination
                    `GET /project-contributions` is cursor-paginated and supports filters by project, amount range, payment method, and date range. There is no member filter — because there is no member on the record. `GET /project-contributions/summary` returns `totalContributed`, `totalContributions`, `averageAmount`, and `mostUsedPaymentMethod`.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Tithes",
                Description = """
                    **Tithes record individual tithe payments made by Members.** Each record captures the member, the amount, the period the tithe is *for* (`forYear` + `forMonth`), the actual `collectionDate`, the payment method, and an optional description.

                    Tithes are a **separate financial resource** from Transactions. A tithe is money given by an identified member and is attributed to that member. A Transaction is the congregation's general income or expense and is not tied to a specific giver.

                    - Use **Tithes** when you need to attribute money to a member (statements, member giving history, per-member totals).
                    - Use **Transactions** for general financial movements that don't need a member link.
                    - Use **Project Contributions** for project-specific giving, where the giver is anonymous.

                    ### Year and month are separate from the collection date
                    A tithe is *for* a specific month (`forYear`, `forMonth`) even if it was collected later. Filtering by `year` and `month` returns tithes for that period regardless of when they were physically received. Filtering by `from`/`to` filters on `collectionDate`.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create links a Member, which must already exist in the same congregation. Update replaces the full record. Delete is a soft delete — the record is excluded from lists and tithe summary totals will reflect the deletion.

                    ### Dependencies
                    Every Tithe references a **Member** in the same congregation. A missing or cross-tenant member returns `404 FOREIGN_KEY_NOT_FOUND`.

                    ### Filters and pagination
                    `GET /tithes` is cursor-paginated and supports filters by member, payment method, year, month, and date range. `GET /tithes/summary` returns `totalCollected`, `uniqueTithers`, `mostUsedPaymentMethod`, and `averagePerMember` for the same filter set.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Transactions",
                Description = """
                    **Transactions record general financial activity for the congregation** — income and expenses outside of tithes and project contributions. Each Transaction belongs to a Transaction Category and carries an amount, a date, a type, and optional notes.

                    Transactions are the congregation's general ledger. They are **not** attributed to a specific member. Echo has three distinct money concepts, and it matters which one you use:

                    | Resource | Attributed to a Member? | Tied to a Project? | Period-scoped? |
                    |---|---|---|---|
                    | Tithes | Yes | No | Yes (year + month) |
                    | Project Contributions | No (anonymous) | Yes | No |
                    | Transactions | No | No | No |

                    - Use **Tithes** for member-attributed giving.
                    - Use **Project Contributions** for project-specific giving.
                    - Use **Transactions** for everything else — general income and expenses.

                    ### Type
                    `TransactionType` is `Income` or `Expense`. There is no separate "expenditure" enum value — the API and UI should use `Expense`.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create a Transaction → optionally update, soft-delete. Update replaces the full record. Delete is a soft delete — the record is excluded from lists and summary totals will reflect the deletion.

                    ### Dependencies
                    Every Transaction must reference a **Transaction Category** in the same congregation. A missing or cross-tenant category returns `404 FOREIGN_KEY_NOT_FOUND`.

                    ### Filters and pagination
                    `GET /transactions` is cursor-paginated and supports filters by category, transaction type, and date range. `GET /transactions/summary` returns `totalIncome`, `totalExpenses`, `net`, and `mostActiveCategory` for the same filter set.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, `FOREIGN_KEY_NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Transaction Categories",
                Description = """
                    **Transaction Categories are congregation-defined classifications for Transactions** — for example Offerings, Utilities, Salaries, Rent.

                    Every Transaction must belong to exactly one Transaction Category. Categories are how the congregation groups its income and expenses for reporting.

                    ### Categories are typed
                    Each category carries a `categoryType` of `Income` or `Expense`. When creating a Transaction, the selected category should match the transaction's `transactionType` — a Salary category is an `Expense` category and should not be attached to an Income transaction. Populate the category picker by filtering on the transaction type the user is recording.

                    ### Lookup entity — no pagination
                    `GET /transaction-categories` returns the **full list** with no pagination. Categories are lightweight and bounded in number. Call this to populate the category selector when creating or updating a Transaction.

                    ### Who can call these endpoints
                    Any authenticated user in the congregation.

                    ### Lifecycle
                    Create a category before creating any Transaction that references it. Update replaces the full record — **existing Transactions assigned to that category retain their assignment**. Delete is a soft delete: the category disappears from lists and lookups, but existing Transactions that reference it are not affected.

                    ### Dependencies
                    Nothing. This is a leaf lookup entity within a congregation.

                    ### Search
                    `GET /transaction-categories/search` is a flat, rate-limited trigram search over category names. The response includes the category `type` so the picker can filter by Income/Expense.

                    ### Errors
                    See the error catalog for `VALIDATION_ERROR`, `NOT_FOUND`, and `UNAUTHORIZED`.
                    """,
            },
            new()
            {
                Name = "Health",
                Description = """
                    Liveness and readiness probes for infrastructure monitoring.

                    - `GET /api/health/live` — the process is running.
                    - `GET /api/health/ready` — dependencies (database, cache) are reachable.

                    Both endpoints are unauthenticated and are intended for load balancers, orchestrators, and uptime monitors. They return `200` on success with no body. Do not call them from frontend applications.
                    """,
            },
        };
    }
}
