using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Auth.Registrations;

public static class RegistrationEndpoints
{
    public static RouteGroupBuilder MapRegistrationEndpoints(
        this RouteGroupBuilder group,
        AuthInstrumentation instrumentation
    )
    {
        var reg = group
            .MapGroup("/registrations")
            .WithTags("Registrations")
            .RequireRateLimiting("auth");

        reg.MapPost(
                "/congregation",
                async (
                    RegisterCongregationRequest request,
                    RegistrationService svc,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.registration.congregation"
                    );
                    return (
                        await svc.RegisterCongregation(request.CongregationDto, request.UserDto, ct)
                    ).ToResult();
                }
            )
            .WithName("RegisterCongregation")
            .WithSummary("Registers a new congregation and its initial administrator.")
            .WithDescription(
                """
                Creates a brand-new congregation together with its first administrator account, in a single transaction. This is the entry point for onboarding a new tenant into Echo.

                ### When to use this
                Only when the congregation does not yet exist in Echo. If the congregation is already registered and you want to add another user to it, use `POST /users` (authenticated) or the invitation flow instead — not this endpoint.

                ### Request body
                Two nested objects, both required.

                `congregationDto` — the congregation's profile. Required fields:
                - `name` — the congregation's display name.
                - `phoneNumber` — contact phone for the congregation.
                - `emailAddress` — contact email for the congregation. This is the **congregation's** email, not the admin's.
                - `city`
                - `town`
                - `gpsAddress`

                Optional: `orgType` (`Church`, `Mosque`, `NonProfit`, `Other`), `postalAddress`, `websiteUrl`, `region` (one of the sixteen Ghana regions).

                `userDto` — the first administrator's account. Required fields:
                - `firstName`, `lastName`
                - `emailAddress` — the admin's personal email. Must be unique across the platform; the same email cannot be reused for another account in any congregation.
                - `password` — minimum 8 characters.

                Optional: `otherNames`, `role`. The `role` here is normally `Admin` for the initial user, but the server may force that regardless of what is supplied.

                ### On success
                Returns `200 OK` with no body.

                **The admin is not logged in.** Registration does not issue a session. The newly created admin must:
                1. Receive and complete email verification via the link sent to the supplied admin email.
                2. Sign in via `POST /auth/sessions/login` once verification is complete.

                Do not assume the response carries tokens — it does not. Send the user to a "check your email" screen.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing required fields, wrong types).
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object — each entry is keyed by field path, so nested failures appear as `congregationDto.emailAddress` or `userDto.password`.
                - `409 CONFLICT` — a congregation with the supplied name, or a user with the supplied admin email, already exists.

                ### Side effects
                - A new congregation is created.
                - A new user account is created with the `Admin` role and scoped to the new congregation.
                - A verification email is sent to the admin's email address.
                - No session is created.

                ### Authentication
                None required. This is a public onboarding endpoint.

                ### Rate limiting
                This endpoint is rate-limited per the `auth` policy. Excessive requests return `429 Too Many Requests`.
                """
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        reg.MapPost(
                "/member",
                async (
                    RegisterMemberRequest request,
                    RegistrationService svc,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.registration.member"
                    );
                    return (await svc.RegisterUser(request, ct)).ToResult();
                }
            )
            .WithName("RegisterMember")
            .WithSummary("Registers a new member account.")
            .WithDescription(
                """
                Creates a new user account in an existing congregation by redeeming an invitation token. This is the second half of the invitation flow — the first half is `POST /auth/invitations`, which mints the token.

                ### When to use this
                The invited user has received an invitation token out of band (email, chat, SMS) and is submitting it along with their account details to complete signup. There is no way to register a new member without a valid invitation token — Echo does not support open self-registration.

                ### Request body
                - `token` — **required.** The invitation token from `POST /auth/invitations`. Extract it however the admin delivered it. The token is single-use and expiring.
                - `userInfo` — **required.** A `UserCreateDto` describing the new account:
                  - `firstName`, `lastName` — required.
                  - `emailAddress` — required. Must be unique within the congregation.
                  - `password` — required, minimum 8 characters.
                  - `otherNames` — optional.
                  - `role` — present on the DTO for symmetry, but **the role is taken from the invitation, not from this field.** Whatever the admin set as `allowedRole` on the invitation is what the account receives. Supplying a different `role` here has no effect. Do not build UI that lets the invited user pick their own role.

                ### On success
                Returns `200 OK` with no body.

                **The user is not logged in.** Registration does not issue a session. The new account must:
                1. Receive and complete email verification via the link sent to `emailAddress`.
                2. Sign in via `POST /auth/sessions/login` once verification is complete.

                Send the user to a "check your email" screen.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing fields, wrong types).
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object. Common cases: password too short, invalid email format.
                - `401 INVALID_TOKEN` — the invitation token is not found, has expired, or has already been used. All three cases return this same code. Route the user to contact the admin who invited them for a fresh invitation.
                - `409 CONFLICT` — a user with the supplied email address already exists in this congregation. **This check runs after the token check**, so a valid token combined with a duplicate email returns `409`, not `401`.

                ### Side effects
                - A new user account is created in the congregation the invitation was scoped to.
                - The role assigned is the invitation's `allowedRole`, not whatever `userInfo.role` says.
                - The invitation token is marked as used and cannot be reused.
                - A verification email is sent to the supplied email address.
                - No session is created.

                ### Authentication
                None required. The invitation token is the credential.

                ### Rate limiting
                This endpoint is rate-limited per the `auth` policy.
                """
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
