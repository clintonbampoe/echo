using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Api.Extensions;

public static class ExceptionHandlingExtensions
{
    public static WebApplication ConfigureExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
                var isDevelopment = env.IsDevelopment();

                context.Response.ContentType = "application/problem+json";

                if (exceptionFeature?.Error is BadHttpRequestException badRequest)
                {
                    await WriteBadRequest(context, badRequest, isDevelopment);
                    return;
                }

                await WriteServerError(context, exceptionFeature?.Error, isDevelopment);
            });
        });

        return app;
    }

    private static async Task WriteBadRequest(
        HttpContext context,
        BadHttpRequestException exception,
        bool isDevelopment
    )
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = ErrorTypes.BadRequest,
            Title = "Bad Request",
            Detail = exception.Message,
            Instance = context.Request.Path,
        };
        problem.Extensions["errorCode"] = "BAD_REQUEST";

        if (isDevelopment && exception.InnerException is not null)
        {
            problem.Extensions["innerException"] = exception.InnerException.Message;
            problem.Extensions["innerExceptionType"] = exception.InnerException.GetType().FullName;
        }

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static async Task WriteServerError(
        HttpContext context,
        Exception? exception,
        bool isDevelopment
    )
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = ErrorTypes.InternalServerError,
            Title = "Internal Server Error",
            Detail = isDevelopment
                ? exception?.Message
                : "Something went wrong while processing your request.",
            Instance = context.Request.Path,
        };
        problem.Extensions["errorCode"] = "INTERNAL_SERVER_ERROR";

        if (isDevelopment && exception is not null)
        {
            problem.Extensions["exceptionType"] = exception.GetType().FullName;
            problem.Extensions["stackTrace"] = exception.StackTrace;
            problem.Extensions["innerExceptions"] = FlattenInnerExceptions(exception);
        }

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static string[] FlattenInnerExceptions(Exception exception)
    {
        var chain = new List<string>();
        var current = exception.InnerException;

        while (current is not null)
        {
            chain.Add($"{current.GetType().FullName}: {current.Message}");
            current = current.InnerException;
        }

        return [.. chain];
    }
}
