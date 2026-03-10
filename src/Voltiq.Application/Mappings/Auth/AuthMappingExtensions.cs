using Voltiq.Application.Features.Auth.Commands.Login;

namespace Voltiq.Application.Mappings.Auth;

public static class AuthMappingExtensions
{
    extension(LoginRequest request)
    {
        public LoginCommand ToCommand() =>
            new(request.Email, request.Password);
    }
}
