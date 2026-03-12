using System.Diagnostics.CodeAnalysis;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Voltiq.Exceptions.Resources;

namespace Voltiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    [field: AllowNull, MaybeNull]
    protected ISender Sender =>
        field ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult ToErrorResult(List<Error>? errors)
    {
        if (errors is null || errors.Count == 0)
        {
            return Problem(title: ResourceErrorMessages.TITULO_ERRO_INESPERADO, statusCode: 500);
        }

        return errors.All(error => error.Type == ErrorType.Validation) ? 
            BuildValidationProblem(errors) : 
            BuildProblem(errors[0]);
    }

    private ObjectResult BuildProblem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var title = error.Type switch
        {
            ErrorType.Conflict => ResourceErrorMessages.TITULO_CONFLITO,
            ErrorType.NotFound => ResourceErrorMessages.TITULO_NAO_ENCONTRADO,
            ErrorType.Unauthorized => ResourceErrorMessages.TITULO_NAO_AUTORIZADO,
            _ => ResourceErrorMessages.TITULO_ERRO_INESPERADO
        };

        return Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Description);
    }

    private ActionResult BuildValidationProblem(List<Error> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        foreach (var error in errors)
        {
            modelStateDictionary.AddModelError(
                error.Code,
                error.Description);
        }
        
        return ValidationProblem(modelStateDictionary);
    }
}
