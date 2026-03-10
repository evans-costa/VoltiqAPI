using MediatR;
using Voltiq.Domain.Common;

namespace Voltiq.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;
