using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Clients.Queries.GetClientById;

public sealed record GetClientByIdQuery(Guid Id) : IRequest<ErrorOr<ClientResponse>>;
