using System.Diagnostics.CodeAnalysis;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Exceptions.Resources;

namespace Voltiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    [field: AllowNull, MaybeNull]
    protected ISender Sender =>
        field ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult ToErrorResult<T>(ErrorOr<T> result)
    {
        var firstError = result.FirstError;

        return firstError.Type switch
        {
            ErrorType.Validation => BuildValidationProblem(result.Errors),

            ErrorType.NotFound => Problem(
                title: ResourceErrorMessages.TITULO_NAO_ENCONTRADO,
                detail: firstError.Description,
                statusCode: StatusCodes.Status404NotFound,
                instance: HttpContext.Request.Path),

            ErrorType.Conflict => Problem(
                title: ResourceErrorMessages.TITULO_CONFLITO,
                detail: firstError.Description,
                statusCode: StatusCodes.Status409Conflict,
                instance: HttpContext.Request.Path),

            ErrorType.Unauthorized => Problem(
                title: ResourceErrorMessages.TITULO_NAO_AUTORIZADO,
                detail: firstError.Description,
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path),

            _ => Problem(
                title: ResourceErrorMessages.TITULO_ERRO_INESPERADO,
                statusCode: StatusCodes.Status500InternalServerError,
                instance: HttpContext.Request.Path)
        };
    }

    private ObjectResult BuildValidationProblem(List<Error> errors)
    {
        var dict = new Dictionary<string, string[]>();

        foreach (var error in errors.Where(e => e.Type == ErrorType.Validation))
        {
            if (!dict.ContainsKey(error.Code))
                dict[error.Code] = [];

            dict[error.Code] = [.. dict[error.Code], error.Description];
        }

        var problemDetails = new ValidationProblemDetails(dict)
        {
            Title = ResourceErrorMessages.TITULO_FALHA_VALIDACAO,
            Status = StatusCodes.Status400BadRequest,
            Instance = HttpContext.Request.Path,
        };

        return new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
    }
}
