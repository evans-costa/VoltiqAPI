using Voltiq.Application.Features.Users.Commands.RegisterUser;
using Voltiq.Application.Features.Users.Queries.GetCurrentUser;
using Voltiq.Domain.Entities;

namespace Voltiq.Application.Mappings.Users;

public static class UserMappingExtensions
{
    extension(RegisterUserRequest request)
    {
        public RegisterUserCommand ToCommand() =>
            new(request.Name, request.Email, request.Document, request.Password);
    }

    extension(User user)
    {
        public RegisterUserResponse ToRegisterUserResponse(string accessToken, string refreshToken) =>
            new(user.Id, accessToken, refreshToken);

        public GetUserResponse ToGetUserResponse() =>
            new(user.Name, user.Email.Value);
    }
}
