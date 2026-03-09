namespace Voltiq.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserRequest(
    string Name,
    string Email,
    string Document,
    string Password);
