using MediatR;
using Voltiq.Application.Features.Auth.Commands.Login;
using Voltiq.Domain.Common;

namespace Voltiq.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;
