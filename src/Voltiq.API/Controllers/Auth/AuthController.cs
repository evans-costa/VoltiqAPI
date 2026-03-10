using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Auth.Commands.Login;

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
}
