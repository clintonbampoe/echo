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
            .MapPost(
                "/account",
                async (
                    EmailVerificationLinkRequest request,
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
            .Produces(StatusCodes.Status200OK);

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
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
