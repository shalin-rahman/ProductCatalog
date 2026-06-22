using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog.Api.Middleware;

/// <summary>
/// A global exception handler using the modern .NET 8 IExceptionHandler interface.
/// This catches any unhandled exceptions thrown by controllers or services, logs them, 
/// and returns a clean JSON ProblemDetails response instead of an HTML stack trace.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the full stack trace and error message
        _logger.LogError(exception, "A critical error occurred: {Message}", exception.Message);

        // 2. Create a clean JSON response using RFC 7807 Problem Details
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing your request. Please try again later."
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        // 3. Write the JSON response back to the client
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to signal that this exception is fully handled
        return true; 
    }
}
