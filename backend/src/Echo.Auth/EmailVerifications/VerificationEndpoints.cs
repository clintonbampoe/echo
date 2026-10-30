using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Auth.EmailVerifications;

public static class EmailVerificationEndpoints
{
    public static RouteGroupBuilder MapVerificationEndpoints(
        this RouteGroupBuilder group,
        AuthInstrumentation instrumentation
    )
    {
        var verifications = group
            .MapGroup("/verifications")
            .WithTags("Verifications")
            .RequireRateLimiting("auth");

        verifications
            .MapGet(
                "/status",
                async (EmailVerificationService svc, HttpContext context, CancellationToken ct) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.verification.status"
                    );
                    return (
                        await svc.GetStatus(
                            context.User.GetCongregationId(),
                            context.User.GetUserId(),
                            ct
                        )
                    ).ToResult();
                }
            )
            .RequireAuthorization()
            .WithName("GetVerificationStatus")
            .WithSummary("Says whether the signed-in user's email address is verified.")
            .WithDescription(
                """
                Tells the client whether to keep reminding the signed-in user to verify their email. Signing in does not need a verified address, so this is how the app finds out.

                ### Response fields
                - `verified` — `true` once the address has been verified.
                - `verifiedAt` — when it was verified (UTC); `null` while it is not.
                - `emailAddress` — the address a verification link goes to. To send one, call `POST /auth/verifications/account` with this address.

                The same `verifiedAt` is also on `GET /users/me`. Use this endpoint to re-check cheaply, for example when the user returns to the app after clicking the link in their email.

                This endpoint takes no parameters. Nothing is restricted for an unverified user.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — the account no longer exists.
                - `429` — rate limited per the `auth` policy.
                """
            )
            .Produces<VerificationStatusDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        verifications
            .MapPost(
                "/account",
                async (
                    [FromBody] EmailVerificationLinkRequest request,
                    EmailVerificationService svc,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.verification.send"
                    );
                    return (await svc.SendVerificationLinkToEmail(request.Email, ct)).ToResult();
                }
            )
            .WithName("SendVerificationLink")
            .WithSummary("Sends an email verification link.")
            .WithDescription(
                """
                Starts the email verification flow. If an account exists for the supplied email address and has not yet been verified, a verification link is sent to that address. If the address is already verified or does not exist, nothing is sent — but the response is identical.

                ### When to use this
                - Immediately after registration, if the registration flow does not auto-send the first link.
                - From a reminder inside the app for a signed-in user whose `GET /auth/verifications/status` says `verified: false`. Signing in does not need a verified address.
                - From an account settings page for a user who never completed verification.

                ### Request body
                - `email` — the address to send the verification link to. This is the address on the account, not an arbitrary address.

                ### On success
                Returns `200 OK` with no body, **whether or not the email is registered or already verified**.

                This is deliberate. The response does not reveal whether the address exists or whether it has already been verified, to prevent account enumeration. The frontend must not branch its UI messaging on the response — always show the same confirmation:

                > If that email is registered and unverified, a verification link is on its way.

                Do not change that copy based on anything the API returns, because the API will not tell you.

                ### Authentication
                None required. This endpoint is called before a session exists.

                ### What the link contains
                The email delivers a URL that includes a `token` **as a query string parameter**, not in the body. The frontend should extract it from the URL and submit it via `POST /auth/verifications/verify-email?token=...`. Tokens are **single-use and expire** — the exact lifetime is configured server-side.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing `email`).
                - `400 VALIDATION_ERROR` — the email field failed format validation. Inspect the `errors` object.

                No failure mode leaks whether the account exists or is already verified.

                ### Rate limiting
                This endpoint is rate-limited per the `auth` policy. Excessive requests return `429 Too Many Requests`. Throttle the "resend" button in the UI to match.
                """
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        verifications
            .MapPost(
                "/verify-email",
                async (
                    [FromQuery] string token,
                    EmailVerificationService svc,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.verification.verify"
                    );
                    return (await svc.VerifyEmail(token, ct)).ToResult();
                }
            )
            .WithName("VerifyEmail")
            .WithSummary("Verifies an email address using a token.")
            .WithDescription(
                """
                Confirms an email address using the token delivered by the verification email. Marks the account as verified.

                ### The token is a query parameter, not a body
                **This POST endpoint takes `token` as a query string parameter**, not in the request body. Example: `POST /auth/verifications/verify-email?token=<token-from-email>`

                There is no request body. Do not send the token as JSON — it will not be read. This is unusual for a POST, so call it out if you are generating a client from the OpenAPI spec; the token will appear under `parameters`, not `requestBody`.

                ### When to use this
                The user clicks the link in the verification email, the client extracts the `token` from the URL, and calls this endpoint. On success, the account is verified.

                ### On success
                Returns `200 OK` with no body. The account's `verifiedAt` timestamp is populated.

                The verification endpoint is unauthenticated, so it does not issue a session. If the user is signed in, re-check `GET /auth/verifications/status`; if not, route them to the login screen.

                ### Failure modes
                - `400 BAD_REQUEST` — the `token` query parameter is missing entirely.
                - `401 INVALID_TOKEN` — the token is not found, has expired, or has already been used. All three cases return this same code. Prompt the user to request a new verification link via `POST /auth/verifications/account`.

                ### Side effects
                - The verification token is marked as used and cannot be reused.
                - The account's `verifiedAt` is set.

                ### Authentication
                None required. The token is the credential.

                ### Rate limiting
                This endpoint is rate-limited per the `auth` policy.
                """
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return group;
    }
}
