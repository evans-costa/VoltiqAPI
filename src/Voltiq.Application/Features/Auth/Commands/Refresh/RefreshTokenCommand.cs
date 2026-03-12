using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<AuthResponse>>;
