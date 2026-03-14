using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Document,
    string Password) : IRequest<ErrorOr<RegisterUserResponse>>;
