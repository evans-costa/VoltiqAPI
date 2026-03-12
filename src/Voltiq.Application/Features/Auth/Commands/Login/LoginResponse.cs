namespace Voltiq.Application.Features.Auth.Commands.Login;

public sealed record LoginResponse(string AccessToken, string RefreshToken);
