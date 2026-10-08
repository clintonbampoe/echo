using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Auth.Passwords;

public static class PasswordEndpoints
{
    public static RouteGroupBuilder MapPasswordEndpoints(
        this RouteGroupBuilder group,
        AuthInstrumentation instrumentation
    )
    {
        var passwords = group
            .MapGroup("/passwords")
            .WithTags("Passwords")
            .RequireRateLimiting("auth");

        passwords
            .MapPost(
                "/forgot",
                async (
                    ForgotPasswordRequest request,
                    PasswordResetService svc,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.password.forgot"
                    );
                    return (await svc.SendForgotPasswordLinkToEmail(request.Email, ct)).ToResult();
                }
            )
            .WithName("ForgotPassword")
            .WithSummary("Sends a password reset link if the email exists.")
            .WithDescription(
                """
                Starts the password reset flow. If an account exists for the supplied email address, a reset link is sent to that address. If no account exists, nothing is sent — but the response is identical.

                ### Request body
                - `email` — the address to send the reset link to.

                ### On success
                Returns `200 OK` with no body, **whether or not the email is registered**.

                This is deliberate. The response does not reveal whether the address exists, to prevent account enumeration. The frontend must not branch its UI messaging on the response — always show the same confirmation, e.g.:

                > If an account exists for that email, you'll receive a reset link shortly.

                Do not change that copy based on anything the API returns, because the API will not tell you.

                ### Authentication
                None required. This endpoint exists precisely because the user cannot sign in.

                ### What the link contains
                The email delivers a URL that includes a `token`. The frontend should extract the token from that URL and submit it to `POST /auth/passwords/reset` along with the new password. Tokens are **single-use and expire** — the exact lifetime is configured server-side and is not exposed to the client.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing `email`, or the field is not a valid email string).
                - `400 VALIDATION_ERROR` — the email field failed validation. Inspect the `errors` object.

                No failure mode leaks whether the account exists.

                ### Rate limiting
                This endpoint is rate-limited per the `auth` policy. Excessive requests return `429 Too Many Requests`. Apply the same treatment to the UI — throttle the "resend" button.
                """
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        passwords
            .MapPost(
                "/reset",
                async (
                    PasswordResetRequest request,
                    PasswordResetService svc,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.password.reset"
                    );
                    return (
                        await svc.ResetPassword(request.Token, request.NewPassword, ct)
                    ).ToResult();
                }
            )
            .WithName("ResetPassword")
            .WithSummary("Resets a password using a reset token.")
            .WithDescription(
                """
                Completes the password reset flow started by `POST /auth/passwords/forgot`. Consumes the reset token and sets the account's new password.

                ### Request body
                - `token` — **required.** The token from the reset link. Extract it from the URL the user clicked; do not modify it.
                - `newPassword` — **required.** The new password. Must be at least 8 characters. The server may enforce additional complexity rules; violations return `400 VALIDATION_ERROR` with the specifics in the `errors` object.
                - `email` — present on the request shape for symmetry with the forgot endpoint, but **not used to process the reset.** The token alone identifies the account. Do not rely on it for anything.

                ### On success
                Returns `200 OK` with a body containing the message `"Password reset successfully"`. The password is changed and the reset token is consumed.

                The user is **not** logged in automatically. After success, send them to the login screen and have them sign in with the new password. The reset endpoint is unauthenticated, so it does not issue a session.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing `token` or `newPassword`).
                - `400 VALIDATION_ERROR` — the new password failed the server's password policy. Inspect the `errors` object for the specific rule that was violated.
                - `401 INVALID_TOKEN` — the token is not found, has expired, or has already been used. All three cases return this same code. Prompt the user to request a new reset link.

                ### Side effects
                - The reset token is marked as used and cannot be reused.
                - The account's password hash is replaced.
                - **Every active session for the user is revoked.** All refresh tokens issued to this account are invalidated. Anyone currently signed in — including the user on other devices, and any attacker who held a stolen token — will be forced to log in again on their next token refresh.

                The frontend should message this clearly. A common pattern is: *"Your password has been reset. For security, you've been signed out of all devices. Please sign in again."*

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
