using MediatR;
using Voltiq.Domain.Common;

namespace Voltiq.Application.Features.Users.Queries.GetUser;

public sealed record GetUserQuery(Guid Id) : IRequest<Result<GetUserResponse>>;
