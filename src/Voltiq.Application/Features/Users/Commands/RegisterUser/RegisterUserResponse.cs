namespace Voltiq.Application.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserResponse(Guid Id, string AccessToken, string RefreshToken);
