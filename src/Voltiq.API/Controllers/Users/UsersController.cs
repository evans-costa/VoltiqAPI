using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Users.Commands.CreateUser;
using Voltiq.Application.Features.Users.Queries.GetUser;

namespace Voltiq.API.Controllers.Users;

public sealed class UsersController : BaseApiController
{
    /// <summary>Creates a new user account.</summary>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="409">Email and/or document already in use.</response>
    [AllowAnonymous]
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

        return result.IsFailure ?
            ToErrorResult(result) :
            CreatedAtAction(nameof(Create), new { id = result.Value.Id }, result.Value);
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
        var result = await Sender.Send(new GetUserQuery(id), cancellationToken);

        return result.IsFailure ? ToErrorResult(result) : Ok(result.Value);
    }
}
