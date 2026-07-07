using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Gclopes.FinVault.PensionPlans.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    // Intercept and gracefully process any unhandled application runtime failure across execution pipelines
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the intercepted runtime crash using critical severity logging levels alongside error metadata
        _logger.LogError(exception, "An unhandled execution exception occurred inside the CurrentAccount pipeline: {Message}", exception.Message);

        // Standardize the network payload structure using the official ProblemDetails application layout schema
        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "An unexpected server error occurred",
            Detail = exception.Message, // Note: In production environments, replace this with a generic message to prevent data leaks
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        // Enforce the standard application/problem+json content delivery specifications onto client responses
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        // Dispatch the finalized serialized error payload downstream back to the calling network clients
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Instruct the underlying ASP.NET Core request pipeline engine to halt further error bubble processing
        return true;
    }
}
