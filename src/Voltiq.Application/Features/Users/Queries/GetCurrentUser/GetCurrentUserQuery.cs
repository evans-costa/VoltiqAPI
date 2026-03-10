using MediatR;
using Voltiq.Application.Features.Users.Queries.GetUser;
using Voltiq.Domain.Common;

namespace Voltiq.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<GetUserResponse>>;
