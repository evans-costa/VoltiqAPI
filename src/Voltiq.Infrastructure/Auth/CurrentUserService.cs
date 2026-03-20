using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Infrastructure.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var id = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User?.FindFirstValue("sub");

            return Guid.TryParse(id, out var result) ? result : Guid.Empty;
        }
    }

    public string? UserName => User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("unique_name");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
