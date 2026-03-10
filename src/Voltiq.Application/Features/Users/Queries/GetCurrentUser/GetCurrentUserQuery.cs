using MediatR;
using Voltiq.Domain.Common;

namespace Voltiq.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<GetUserResponse>>;
