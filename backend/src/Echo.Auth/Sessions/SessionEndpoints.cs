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
            .Produces<TokenPairResponseDtos>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

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
            .Produces(StatusCodes.Status200OK);

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
            .WithSummary("Rotates a refresh token and issues a new access token.")
            .Produces<TokenPairResponseDtos>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

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
            .Produces(StatusCodes.Status200OK);

        return group;
    }
}
