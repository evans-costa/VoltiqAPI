using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Exceptions.Resources;

namespace Voltiq.API.ExceptionHandlers;

internal sealed class ValidationExceptionHandler(IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        var errors = validationException.Errors
            .Select(e => new ValidationErrorItem(e.PropertyName, e.ErrorMessage))
            .ToArray();

        var response = new
        {
            title = ResourceErrorMessages.Titulo_FalhaValidacao,
            status = StatusCodes.Status400BadRequest,
            instance = httpContext.Request.Path.Value,
            errors,
            traceId = env.IsDevelopment() ? httpContext.TraceIdentifier : null,
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}

file sealed record ValidationErrorItem(
    [property: JsonPropertyName("propertyName")] string PropertyName,
    [property: JsonPropertyName("errorMessage")] string ErrorMessage);

