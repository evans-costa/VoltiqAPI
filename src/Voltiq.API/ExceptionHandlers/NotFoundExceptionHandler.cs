using Microsoft.AspNetCore.Diagnostics;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.API.ExceptionHandlers;

internal sealed class NotFoundExceptionHandler(IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
            return false;

        var response = new
        {
            title = ResourceErrorMessages.Titulo_NaoEncontrado,
            status = StatusCodes.Status404NotFound,
            instance = httpContext.Request.Path.Value,
            detail = string.Format(
                ResourceErrorMessages.Entidade_NaoEncontrada,
                notFoundException.EntityName,
                notFoundException.Key),
            traceId = env.IsDevelopment() ? httpContext.TraceIdentifier : null,
        };

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
