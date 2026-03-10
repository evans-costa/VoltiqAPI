using Voltiq.Application.Features.Users.Commands.CreateUser;
using Voltiq.Application.Features.Users.Queries.GetCurrentUser;
using Voltiq.Domain.Entities;

namespace Voltiq.Application.Mappings.Users;

public static class UserMappingExtensions
{
    extension(CreateUserRequest request)
    {
        public CreateUserCommand ToCommand() =>
            new(request.Name, request.Email, request.Document, request.Password);
    }

    extension(User user)
    {
        public CreateUserResponse ToCreateUserResponse(string token) =>
            new(user.Id, token);

        public GetUserResponse ToGetUserResponse() =>
            new(user.Name, user.Email.Value);
    }
}
