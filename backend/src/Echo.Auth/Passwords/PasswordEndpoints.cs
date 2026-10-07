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
            .Produces(StatusCodes.Status200OK);

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
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
