using MediatR;

namespace Voltiq.Application.Features.Users.Queries.GetUser;

public sealed record GetUserQuery(Guid Id) : IRequest<GetUserResponse>;
