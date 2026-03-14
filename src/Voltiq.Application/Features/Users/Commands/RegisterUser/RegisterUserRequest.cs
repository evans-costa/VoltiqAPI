namespace Voltiq.Application.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserRequest(
    string Name,
    string Email,
    string Document,
    string Password);
