using Microsoft.AspNetCore.Diagnostics;

namespace Voltiq.API.ExceptionHandlers;

internal sealed class UnauthorizedExceptionHandler(IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedAccessException)
            return false;

        var response = new
        {
            title = "Unauthorized",
            status = StatusCodes.Status401Unauthorized,
            instance = httpContext.Request.Path.Value,
            traceId = env.IsDevelopment() ? httpContext.TraceIdentifier : null,
        };

        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
