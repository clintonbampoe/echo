using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Api.Extensions;

public static class Exception
{
    public static WebApplication ConfigureExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var env = context.RequestServices.GetRequiredService<IHostEnvironment>();

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = ErrorTypes.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = env.IsDevelopment()
                        ? exceptionFeature?.Error.Message
                        : "Something went wrong while processing your request.",
                    Instance = context.Request.Path,
                };

                problem.Extensions["errorCode"] = "INTERNAL_SERVER_ERROR";

                await context.Response.WriteAsJsonAsync(problem);
            });
        });

        return app;
    }
}
