namespace Voltiq.Application.Features.Auth.Commands.Refresh;

public sealed record AuthResponse(string AccessToken, string RefreshToken);
