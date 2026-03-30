using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Auth.Commands.Login;
using Voltiq.Application.Features.Auth.Commands.Refresh;
using Voltiq.Application.Features.Users.Commands.RegisterUser;
using Voltiq.Application.Mappings.Auth;
using Voltiq.Application.Mappings.Users;

namespace Voltiq.API.Controllers.Auth;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class AuthController : BaseApiController
{
    /// <summary>Registers a new user account.</summary>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="409">Email and/or document already in use.</response>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            user => CreatedAtAction(nameof(Register), new { id = user.Id }, user),
            ToErrorResult);
    }


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

        return result.Match(
            Ok,
            ToErrorResult);
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

        return result.Match(
            Ok,
            ToErrorResult);
    }
}
