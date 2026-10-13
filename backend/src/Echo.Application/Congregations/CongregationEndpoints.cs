using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Congregations;

public static class CongregationEndpoints
{
    public static IEndpointRouteBuilder MapCongregationEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/congregation").WithTags("Congregation").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    CongregationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.congregation.get"
                    );
                    var result = await service.Get(context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("GetCurrentCongregation")
            .WithSummary("Returns the signed-in user's congregation.")
            .WithDescription(
                """
                Returns the congregation the bearer token belongs to: name, type, contact details, location and when it was created. Any signed-in user can read it. The congregation always comes from the token.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — the congregation no longer exists.
                """
            )
            .Produces<CongregationResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
