using Microsoft.AspNetCore.Diagnostics;
using Voltiq.Exceptions.Resources;

namespace Voltiq.API.ExceptionHandlers;

internal sealed class GlobalExceptionHandler(
    IHostEnvironment env,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        object response = env.IsDevelopment()
            ? new
            {
                title = ResourceErrorMessages.Titulo_ErroInesperado,
                status = StatusCodes.Status500InternalServerError,
                instance = httpContext.Request.Path.Value,
                traceId = httpContext.TraceIdentifier,
                stackTrace = exception.ToString(),
            }
            : (object)new
            {
                title = ResourceErrorMessages.Titulo_ErroInesperado,
                status = StatusCodes.Status500InternalServerError,
                instance = httpContext.Request.Path.Value,
            };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
