using Microsoft.AspNetCore.Mvc;
using Voltiq.API.Features.Users;
using Voltiq.Application.Features.Users.Commands.CreateUser;
using Voltiq.Application.Features.Users.Queries.GetUser;
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
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Name, request.Email, request.Document, request.Password);
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

    /// <summary>Gets a user by ID.</summary>
    /// <response code="200">User found.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var response = await Sender.Send(new GetUserQuery(id), cancellationToken);
        return Ok(response);
    }
}
