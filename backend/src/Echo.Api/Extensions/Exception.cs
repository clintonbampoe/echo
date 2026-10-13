using System.Diagnostics;
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

    internal static async Task WriteBadRequest(
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
        problem.Extensions["traceId"] = GetTraceId(context);

        if (isDevelopment && exception.InnerException is not null)
        {
            problem.Extensions["innerException"] = exception.InnerException.Message;
            problem.Extensions["innerExceptionType"] = exception.InnerException.GetType().FullName;
        }

        await context.Response.WriteAsJsonAsync(problem);
    }

    internal static async Task WriteServerError(
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
        problem.Extensions["traceId"] = GetTraceId(context);

        if (isDevelopment && exception is not null)
        {
            problem.Extensions["exceptionType"] = exception.GetType().FullName;
            problem.Extensions["stackTrace"] = exception.StackTrace;
            problem.Extensions["innerExceptions"] = FlattenInnerExceptions(exception);
        }

        await context.Response.WriteAsJsonAsync(problem);
    }

    // Matches the trace id in logs and OpenTelemetry, so support can find the failing request.
    internal static string GetTraceId(HttpContext context) =>
        Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

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

public static class ValidationProblemExtensions
{
    /// <summary>
    /// Gives validation failures the same fields as every other problem response: status, type,
    /// errorCode and traceId, next to the framework's errors map.
    /// </summary>
    public static IServiceCollection ConfigureValidationProblemDetails(
        this IServiceCollection services
    )
    {
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
            {
                if (context.ProblemDetails is not HttpValidationProblemDetails problem)
                    return;

                problem.Status = StatusCodes.Status400BadRequest;
                problem.Type = ErrorTypes.ValidationError;
                problem.Instance = context.HttpContext.Request.Path;
                problem.Extensions["errorCode"] = "VALIDATION_ERROR";
                problem.Extensions["traceId"] = ExceptionHandlingExtensions.GetTraceId(
                    context.HttpContext
                );
            }
        );

        return services;
    }
}
