using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Users.Commands.CreateUser;
using Voltiq.Application.Mappings.Users;

namespace Voltiq.API.Controllers.Users;

[ApiVersion("1.0")]

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
        var command = request.ToCommand();
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
             user => CreatedAtAction(nameof(Create), new { id = user.Id }, user),
             ToErrorResult);
    }
}
