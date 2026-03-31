using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Users.Queries.GetCurrentUser;

namespace Voltiq.API.Controllers.Users;

[ApiVersion("1.0")]
public sealed class UsersController : BaseApiController
{
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

        return result.Match(
            Ok,
            ToErrorResult);
    }
}
