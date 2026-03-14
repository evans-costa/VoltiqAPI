using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Clients.Queries.GetClients;

public sealed record GetClientsQuery : IRequest<ErrorOr<IReadOnlyList<ClientResponse>>>;
