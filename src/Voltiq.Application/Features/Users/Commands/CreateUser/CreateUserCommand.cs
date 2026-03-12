using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Name,
    string Email,
    string Document,
    string Password) : IRequest<ErrorOr<CreateUserResponse>>;
