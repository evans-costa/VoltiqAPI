using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Domain.Common;
using Voltiq.Exceptions.Errors;
using Voltiq.Exceptions.Resources;

namespace Voltiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    [field: AllowNull, MaybeNull]
    protected ISender Sender =>
        field ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult ToErrorResult(Result result)
    {
        var firstError = result.FirstError;

        return firstError switch
        {
            ValidationError => ValidationProblem(firstError, result),

            NotFoundError notFound => Problem(
                title: ResourceErrorMessages.TITULO_NAO_ENCONTRADO,
                detail: notFound.Message,
                statusCode: StatusCodes.Status404NotFound,
                instance: HttpContext.Request.Path),

            ConflictError conflict => Problem(
                title: ResourceErrorMessages.TITULO_CONFLITO,
                detail: conflict.Message,
                statusCode: StatusCodes.Status409Conflict,
                instance: HttpContext.Request.Path),

            UnauthorizedError unauthorized => Problem(
                title: ResourceErrorMessages.TITULO_NAO_AUTORIZADO,
                detail: unauthorized.Message,
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path),

            _ => Problem(
                title: ResourceErrorMessages.TITULO_ERRO_INESPERADO,
                statusCode: StatusCodes.Status500InternalServerError,
                instance: HttpContext.Request.Path)
        };
    }

    private ObjectResult ValidationProblem(Error _, Result result)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var error in result.Errors.OfType<ValidationError>())
        {
            if (!errors.ContainsKey(error.PropertyName))
                errors[error.PropertyName] = [];

            errors[error.PropertyName] = [.. errors[error.PropertyName], error.Message];
        }

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Title = ResourceErrorMessages.TITULO_FALHA_VALIDACAO,
            Status = StatusCodes.Status400BadRequest,
            Instance = HttpContext.Request.Path,
        };

        return new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
    }
}
