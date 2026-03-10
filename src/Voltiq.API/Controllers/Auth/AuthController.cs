using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Auth.Commands.Login;
using Voltiq.Application.Features.Users.Queries.GetCurrentUser;
using Voltiq.Application.Features.Users.Queries.GetUser;

namespace Voltiq.API.Controllers.Auth;

[Route("auth")]
public sealed class AuthController : BaseApiController
{
    /// <summary>Authenticates a user and returns a JWT token.</summary>
    /// <response code="200">Authentication successful.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await Sender.Send(command, cancellationToken);

        return result.IsFailure ? ToErrorResult(result) : Ok(result.Value);
    }

    /// <summary>Returns the currently authenticated user.</summary>
    /// <response code="200">Current user data.</response>
    /// <response code="401">Token missing or invalid.</response>
    /// <response code="404">User no longer exists.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCurrentUserQuery(), cancellationToken);

        return result.IsFailure ? ToErrorResult(result) : Ok(result.Value);
    }
}
