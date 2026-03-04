namespace Voltiq.API.Features.Users;

public sealed record CreateUserRequest(
    string Name,
    string Email,
    string Document,
    string Password);
