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
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

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
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
