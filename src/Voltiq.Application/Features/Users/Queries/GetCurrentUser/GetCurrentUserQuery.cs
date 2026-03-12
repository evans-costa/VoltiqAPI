using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<ErrorOr<GetUserResponse>>;
