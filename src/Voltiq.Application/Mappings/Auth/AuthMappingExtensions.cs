using Voltiq.Application.Features.Auth.Commands.Login;
using Voltiq.Application.Features.Auth.Commands.Refresh;

namespace Voltiq.Application.Mappings.Auth;

public static class AuthMappingExtensions
{
    extension(LoginRequest request)
    {
        public LoginCommand ToCommand() =>
            new(request.Email, request.Password);
    }

    extension(RefreshTokenRequest request)
    {
        public RefreshTokenCommand ToCommand() =>
            new(request.RefreshToken);
    }
}
