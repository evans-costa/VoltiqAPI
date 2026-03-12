using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Auth.Commands.Login;
using Voltiq.Application.Features.Auth.Commands.Refresh;
using Voltiq.Application.Features.Users.Queries.GetCurrentUser;
using Voltiq.Application.Mappings.Auth;
namespace Voltiq.API.Controllers.Auth;

[Route("auth")]
public sealed class AuthController : BaseApiController
{
    /// <summary>Authenticates a user and returns an access token and a refresh token.</summary>
    /// <response code="200">Authentication successful.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Invalid credentials.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await Sender.Send(command, cancellationToken);

        return result.IsFailure ? ToErrorResult(result) : Ok(result.Value);
    }

    /// <summary>Exchanges a valid refresh token for a new access token and refresh token (rotation).</summary>
    /// <response code="200">Tokens refreshed successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Refresh token invalid, expired or revoked.</response>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.IsFailure ? ToErrorResult(result) : Ok(result.Value);
    }

    /// <summary>Returns the currently authenticated user.</summary>
    /// <response code="200">Current user data.</response>
    /// <response code="401">Token missing or invalid.</response>
    /// <response code="404">User no longer exists.</response>
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
