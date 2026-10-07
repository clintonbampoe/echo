using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Auth.Sessions;

public static class SessionEndpoints
{
    public static RouteGroupBuilder MapSessionEndpoints(
        this RouteGroupBuilder group,
        AuthInstrumentation instrumentation
    )
    {
        var sessions = group.MapGroup("/sessions").WithTags("Sessions").RequireRateLimiting("auth");

        sessions
            .MapPost(
                "/login",
                async (LoginRequest request, SessionService sessionService, CancellationToken ct) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.session.login"
                    );
                    var response = await sessionService.Login(request.Email, request.Password, ct);
                    return response.ToResult();
                }
            )
            .WithName("LoginSession")
            .WithSummary("Authenticates a user and issues a token pair.")
            .WithDescription(
                """
                Exchanges an email and password for a short-lived access token and a long-lived refresh token. This is the only way to start a session.

                ### Request body
                - `email` — the account's verified email address.
                - `password` — the account's current password.

                ### On success
                Returns `200 OK` with a `TokenPairResponseDtos` body:

                - `accessToken` — send as `Authorization: Bearer <token>` on every authenticated request. Do not persist it beyond the current session in memory.
                - `accessTokenExpiresAt` — UTC timestamp. Schedule a refresh before this.
                - `refreshToken` — store securely (httpOnly cookie, secure storage). **Never send as a bearer token.**
                - `refreshTokenExpiresAt` — UTC timestamp. After this, the user must log in again.

                If the device already has a stored refresh token, replace it with the new one — the previous token is not invalidated by a successful login on a different device, so a client that manages multiple sessions should discard the old token locally.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing fields, wrong types).
                - `400 VALIDATION_ERROR` — the email is not a valid email address, or a required field failed its format check. Inspect the `errors` object.
                - `401 INVALID_CREDENTIALS` — the email/password combination is wrong. **Do not reveal which field was wrong** in the UI; the API deliberately returns the same code for both.
                - `403 EMAIL_NOT_VERIFIED` — the credentials are correct but the account has not completed email verification. Route the user into the verification flow (`POST /auth/verifications/account`), not back to the login form.

                ### Side effects
                A new refresh token is created and persisted for the user. All previously issued refresh tokens remain valid — login does not invalidate other sessions. To end all sessions, use `POST /sessions/logout`.

                ### Rate limiting
                This endpoint is rate-limited per the `auth` policy. Excessive requests return `429 Too Many Requests`.
                """
            )
            .Produces<TokenPairResponseDtos>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        sessions
            .MapPost(
                "/logout",
                async (
                    LogoutAllSessionsRequest request,
                    SessionService sessionService,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.session.logout_all"
                    );
                    var response = await sessionService.LogoutOfAllSessions(request.Email, ct);
                    return response.ToResult();
                }
            )
            .WithName("LogoutAllSessions")
            .WithSummary("Terminates every active session for a user.")
            .WithDescription(
                """
                Revokes **every** refresh token issued to the given email. After this call, no refresh token for that account can be exchanged for a new access token — all devices are effectively signed out.

                ### When to use this
                - The user clicked "sign out of all devices".
                - The user is responding to a security incident (suspected token theft).
                - Administrative account lockout.

                For a normal sign-out on a single device, use `POST /sessions/revoke` with the current refresh token instead. Calling `logout` also signs the user out everywhere else.

                ### Request body
                - `email` — the account email whose sessions should be terminated.

                ### On success
                Returns `200 OK` with no body.

                ### Authentication
                None required. The caller is not required to prove possession of any token — this endpoint exists so a user who has lost access to every device can still cut off their sessions.

                ### Side effects
                - Every refresh token for the account is marked revoked.
                - **Access tokens are not blacklisted.** Any access token already issued for this account continues to work until it expires (up to ~15 minutes). The client must discard its access token locally and stop refreshing. See the **Sessions** tag description for the reasoning behind this trade-off.

                ### Idempotency
                Safe to call repeatedly. Revoking an already-revoked session is a no-op and returns `200 OK`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                """
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        sessions
            .MapPost(
                "/refresh",
                async (
                    RefreshTokenRequest request,
                    SessionService sessionService,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.session.refresh"
                    );
                    var response = await sessionService.RefreshAccessToken(
                        request.RefreshToken,
                        ct
                    );
                    return response.ToResult();
                }
            )
            .WithName("RefreshSession")
            .WithSummary("Rotates a refresh token and issues a new token pair.")
            .WithDescription(
                """
                Exchanges a valid refresh token for a fresh token pair. Call this before the current access token expires to keep the user signed in without prompting for credentials.

                ### Request body
                - `refreshToken` — the current refresh token.

                ### Rotation is mandatory
                Refresh tokens are **single-use**. A successful call returns a **new** access token **and** a new refresh token, and immediately invalidates the supplied one.

                The client **must** replace both stored tokens with the values from the response. Calling refresh again with the previous refresh token — including as a retry after a network blip — returns `401 INVALID_TOKEN`. If this happens on a request the client did not intend to be a retry, treat it as a signal that the token was replayed and force re-login.

                ### On success
                Returns `200 OK` with a `TokenPairResponseDtos` body — same shape as login. Use `accessTokenExpiresAt` and `refreshTokenExpiresAt` from the new pair; do not carry forward the old expirations.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing `refreshToken`).
                - `401 INVALID_TOKEN` — the refresh token is expired, revoked, or has already been used. All of these return the same code — the API does not distinguish between them, so the client should not attempt to.

                On `401 INVALID_TOKEN`, discard the stored refresh token and send the user back to login. Do not retry.

                ### Side effects
                - The supplied refresh token is revoked.
                - A new refresh token is issued and persisted.

                ### Authentication
                None required. The refresh token itself is the credential.
                """
            )
            .Produces<TokenPairResponseDtos>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        sessions
            .MapPost(
                "/revoke",
                async (
                    RefreshTokenRequest request,
                    SessionService sessionService,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.session.revoke"
                    );
                    var response = await sessionService.RevokeAccessToken(request.RefreshToken, ct);
                    return response.ToResult();
                }
            )
            .WithName("RevokeSession")
            .WithSummary("Revokes a single refresh token.")
            .WithDescription(
                """
                Revokes one refresh token, ending a single session. Other sessions for the same user — including sessions on other devices — are not affected.

                ### When to use this
                - Sign-out on a single device (client discards its own refresh token by calling this before clearing local state).
                - The user revokes a lost or stolen device from a list of active sessions.

                To sign the user out everywhere, use `POST /sessions/logout` instead.

                ### Request body
                - `refreshToken` — the refresh token to revoke.

                ### On success
                Returns `200 OK` with no body.

                ### Authentication
                None required. Possession of the refresh token is the credential — the caller proves control of the session by presenting it.

                ### Side effects
                - The supplied refresh token is marked revoked.
                - **Access tokens are not blacklisted.** Any access token already issued against this session continues to work until it expires (up to ~15 minutes). The client should discard its access token locally. See the **Sessions** tag description for the reasoning behind this trade-off.

                ### Idempotency
                Safe to call repeatedly. Revoking an already-revoked or unknown token returns `200 OK` — this endpoint does not reveal whether the token was valid to avoid leaking session existence.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing `refreshToken`).
                """
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
