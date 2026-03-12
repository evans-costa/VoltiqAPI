using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<ErrorOr<LoginResponse>>;
