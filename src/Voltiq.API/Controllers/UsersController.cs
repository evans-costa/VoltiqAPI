using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Users.Commands.CreateUser;
using Voltiq.Exceptions.Resources;

namespace Voltiq.API.Controllers;

public sealed class UsersController : BaseApiController
{
    /// <summary>Creates a new user account.</summary>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="409">Email or document already in use.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Conflict(new ProblemDetails
            {
                Title = ResourceErrorMessages.TITULO_CONFLITO,
                Detail = result.Error,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }

        return CreatedAtAction(nameof(Create), new { id = result.Value }, new { id = result.Value });
    }
}
